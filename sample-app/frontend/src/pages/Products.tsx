import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useSearchParams } from 'react-router-dom';
import { Filter, Grid, List, SortAsc } from 'lucide-react';
import { productService, productQueries } from '@/services/productService';
import { ProductFilters } from '@/types';
import { ProductCard } from '@/components/ProductCard';
import { ProductFilters as ProductFiltersComponent } from '@/components/ProductFilters';
import { LoadingSpinner } from '@/components/LoadingSpinner';
import { Pagination } from '@/components/Pagination';

export const Products = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [showFilters, setShowFilters] = useState(false);
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');

  const filters: ProductFilters = {
    page: parseInt(searchParams.get('page') || '1'),
    pageSize: parseInt(searchParams.get('pageSize') || '12'),
    searchTerm: searchParams.get('search') || undefined,
    categoryId: searchParams.get('categoryId') ? parseInt(searchParams.get('categoryId')!) : undefined,
    minPrice: searchParams.get('minPrice') ? parseFloat(searchParams.get('minPrice')!) : undefined,
    maxPrice: searchParams.get('maxPrice') ? parseFloat(searchParams.get('maxPrice')!) : undefined,
    rating: searchParams.get('rating') ? parseInt(searchParams.get('rating')!) : undefined,
    inStock: searchParams.get('inStock') === 'true' ? true : undefined,
    sortBy: (searchParams.get('sortBy') as any) || 'name',
    sortOrder: (searchParams.get('sortOrder') as any) || 'asc',
  };

  const { data, isLoading, error } = useQuery({
    queryKey: productQueries.list(filters),
    queryFn: () => productService.getProducts(filters),
  });

  const updateFilters = (newFilters: Partial<ProductFilters>) => {
    const updatedParams = new URLSearchParams(searchParams);
    
    Object.entries(newFilters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        updatedParams.set(key, value.toString());
      } else {
        updatedParams.delete(key);
      }
    });

    if (!('page' in newFilters)) {
      updatedParams.set('page', '1');
    }

    setSearchParams(updatedParams);
  };

  if (error) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <h2 className="text-2xl font-bold text-gray-900 mb-2">
            Something went wrong
          </h2>
          <p className="text-gray-600 mb-4">
            We couldn't load the products. Please try again.
          </p>
          <button
            onClick={() => window.location.reload()}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            Try Again
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Products</h1>
          {data && (
            <p className="text-gray-600 mt-1">
              Showing {((filters.page! - 1) * filters.pageSize!) + 1}-{Math.min(filters.page! * filters.pageSize!, data.total)} of {data.total} products
            </p>
          )}
        </div>

        <div className="flex items-center space-x-4">
          {/* View Mode Toggle */}
          <div className="flex border border-gray-300 rounded-lg">
            <button
              onClick={() => setViewMode('grid')}
              className={`p-2 ${viewMode === 'grid' ? 'bg-blue-600 text-white' : 'text-gray-600 hover:bg-gray-100'}`}
            >
              <Grid className="w-5 h-5" />
            </button>
            <button
              onClick={() => setViewMode('list')}
              className={`p-2 ${viewMode === 'list' ? 'bg-blue-600 text-white' : 'text-gray-600 hover:bg-gray-100'}`}
            >
              <List className="w-5 h-5" />
            </button>
          </div>

          {/* Sort Dropdown */}
          <select
            value={`${filters.sortBy}-${filters.sortOrder}`}
            onChange={(e) => {
              const [sortBy, sortOrder] = e.target.value.split('-');
              updateFilters({ sortBy: sortBy as any, sortOrder: sortOrder as any });
            }}
            className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          >
            <option value="name-asc">Name (A-Z)</option>
            <option value="name-desc">Name (Z-A)</option>
            <option value="price-asc">Price (Low to High)</option>
            <option value="price-desc">Price (High to Low)</option>
            <option value="rating-desc">Rating (High to Low)</option>
            <option value="newest-desc">Newest First</option>
            <option value="popularity-desc">Most Popular</option>
          </select>

          {/* Filter Toggle */}
          <button
            onClick={() => setShowFilters(!showFilters)}
            className="flex items-center px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
          >
            <Filter className="w-5 h-5 mr-2" />
            Filters
          </button>
        </div>
      </div>

      <div className="flex gap-8">
        {/* Filters Sidebar */}
        {showFilters && (
          <div className="w-64 flex-shrink-0">
            <ProductFiltersComponent
              filters={filters}
              onFiltersChange={updateFilters}
            />
          </div>
        )}

        {/* Products Grid/List */}
        <div className="flex-1">
          {isLoading ? (
            <div className="flex justify-center py-12">
              <LoadingSpinner size="lg" />
            </div>
          ) : data?.data.length === 0 ? (
            <div className="text-center py-12">
              <h3 className="text-lg font-medium text-gray-900 mb-2">
                No products found
              </h3>
              <p className="text-gray-600">
                Try adjusting your filters or search terms.
              </p>
            </div>
          ) : (
            <>
              <div className={
                viewMode === 'grid'
                  ? 'grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6'
                  : 'space-y-4'
              }>
                {data?.data.map((product) => (
                  <ProductCard
                    key={product.id}
                    product={product}
                    viewMode={viewMode}
                  />
                ))}
              </div>

              {/* Pagination */}
              {data && data.totalPages > 1 && (
                <div className="mt-8">
                  <Pagination
                    currentPage={filters.page!}
                    totalPages={data.totalPages}
                    onPageChange={(page) => updateFilters({ page })}
                  />
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
};
