import { useState, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { X, ChevronDown, ChevronUp } from 'lucide-react';
import { ProductFilters as ProductFiltersType, Category } from '@/types';
import { categoryService } from '@/services/categoryService';

interface ProductFiltersProps {
  filters: ProductFiltersType;
  onFiltersChange: (filters: Partial<ProductFiltersType>) => void;
}

export const ProductFilters = ({ filters, onFiltersChange }: ProductFiltersProps) => {
  const [expandedSections, setExpandedSections] = useState({
    categories: true,
    price: true,
    rating: true,
    availability: true,
  });

  const { data: categories } = useQuery({
    queryKey: ['categories'],
    queryFn: () => categoryService.getCategories(),
  });

  const toggleSection = (section: keyof typeof expandedSections) => {
    setExpandedSections(prev => ({
      ...prev,
      [section]: !prev[section],
    }));
  };

  const clearFilters = () => {
    onFiltersChange({
      categoryId: undefined,
      minPrice: undefined,
      maxPrice: undefined,
      rating: undefined,
      inStock: undefined,
      searchTerm: undefined,
    });
  };

  const hasActiveFilters = !!(
    filters.categoryId ||
    filters.minPrice ||
    filters.maxPrice ||
    filters.rating ||
    filters.inStock !== undefined ||
    filters.searchTerm
  );

  return (
    <div className="bg-white rounded-lg shadow-sm border p-6">
      <div className="flex items-center justify-between mb-6">
        <h3 className="text-lg font-semibold text-gray-900">Filters</h3>
        {hasActiveFilters && (
          <button
            onClick={clearFilters}
            className="text-sm text-blue-600 hover:text-blue-700 font-medium"
          >
            Clear All
          </button>
        )}
      </div>

      <div className="space-y-6">
        {/* Search */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Search
          </label>
          <div className="relative">
            <input
              type="text"
              value={filters.searchTerm || ''}
              onChange={(e) => onFiltersChange({ searchTerm: e.target.value || undefined })}
              placeholder="Search products..."
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
            {filters.searchTerm && (
              <button
                onClick={() => onFiltersChange({ searchTerm: undefined })}
                className="absolute right-2 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-600"
              >
                <X className="w-4 h-4" />
              </button>
            )}
          </div>
        </div>

        {/* Categories */}
        <div>
          <button
            onClick={() => toggleSection('categories')}
            className="flex items-center justify-between w-full text-sm font-medium text-gray-700 mb-2"
          >
            Categories
            {expandedSections.categories ? (
              <ChevronUp className="w-4 h-4" />
            ) : (
              <ChevronDown className="w-4 h-4" />
            )}
          </button>
          
          {expandedSections.categories && (
            <div className="space-y-2 max-h-48 overflow-y-auto">
              <label className="flex items-center">
                <input
                  type="radio"
                  name="category"
                  checked={!filters.categoryId}
                  onChange={() => onFiltersChange({ categoryId: undefined })}
                  className="mr-2 text-blue-600 focus:ring-blue-500"
                />
                <span className="text-sm text-gray-700">All Categories</span>
              </label>
              
              {categories?.map((category) => (
                <label key={category.id} className="flex items-center">
                  <input
                    type="radio"
                    name="category"
                    checked={filters.categoryId === category.id}
                    onChange={() => onFiltersChange({ categoryId: category.id })}
                    className="mr-2 text-blue-600 focus:ring-blue-500"
                  />
                  <span className="text-sm text-gray-700">
                    {category.name} ({category.productCount})
                  </span>
                </label>
              ))}
            </div>
          )}
        </div>

        {/* Price Range */}
        <div>
          <button
            onClick={() => toggleSection('price')}
            className="flex items-center justify-between w-full text-sm font-medium text-gray-700 mb-2"
          >
            Price Range
            {expandedSections.price ? (
              <ChevronUp className="w-4 h-4" />
            ) : (
              <ChevronDown className="w-4 h-4" />
            )}
          </button>
          
          {expandedSections.price && (
            <div className="space-y-3">
              <div className="flex space-x-2">
                <div className="flex-1">
                  <label className="block text-xs text-gray-600 mb-1">Min</label>
                  <input
                    type="number"
                    value={filters.minPrice || ''}
                    onChange={(e) => onFiltersChange({ 
                      minPrice: e.target.value ? parseFloat(e.target.value) : undefined 
                    })}
                    placeholder="0"
                    min="0"
                    className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
                <div className="flex-1">
                  <label className="block text-xs text-gray-600 mb-1">Max</label>
                  <input
                    type="number"
                    value={filters.maxPrice || ''}
                    onChange={(e) => onFiltersChange({ 
                      maxPrice: e.target.value ? parseFloat(e.target.value) : undefined 
                    })}
                    placeholder="1000"
                    min="0"
                    className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
              </div>
              
              {/* Quick price ranges */}
              <div className="space-y-1">
                {[
                  { label: 'Under $25', min: 0, max: 25 },
                  { label: '$25 - $50', min: 25, max: 50 },
                  { label: '$50 - $100', min: 50, max: 100 },
                  { label: '$100 - $200', min: 100, max: 200 },
                  { label: 'Over $200', min: 200, max: undefined },
                ].map((range) => (
                  <button
                    key={range.label}
                    onClick={() => onFiltersChange({ 
                      minPrice: range.min, 
                      maxPrice: range.max 
                    })}
                    className={`block w-full text-left px-2 py-1 text-xs rounded hover:bg-gray-100 ${
                      filters.minPrice === range.min && filters.maxPrice === range.max
                        ? 'bg-blue-50 text-blue-700'
                        : 'text-gray-700'
                    }`}
                  >
                    {range.label}
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Rating */}
        <div>
          <button
            onClick={() => toggleSection('rating')}
            className="flex items-center justify-between w-full text-sm font-medium text-gray-700 mb-2"
          >
            Minimum Rating
            {expandedSections.rating ? (
              <ChevronUp className="w-4 h-4" />
            ) : (
              <ChevronDown className="w-4 h-4" />
            )}
          </button>
          
          {expandedSections.rating && (
            <div className="space-y-2">
              {[4, 3, 2, 1].map((rating) => (
                <label key={rating} className="flex items-center">
                  <input
                    type="radio"
                    name="rating"
                    checked={filters.rating === rating}
                    onChange={() => onFiltersChange({ rating })}
                    className="mr-2 text-blue-600 focus:ring-blue-500"
                  />
                  <div className="flex items-center">
                    {[...Array(5)].map((_, i) => (
                      <svg
                        key={i}
                        className={`w-4 h-4 ${
                          i < rating ? 'text-yellow-400 fill-current' : 'text-gray-300'
                        }`}
                        viewBox="0 0 20 20"
                      >
                        <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                      </svg>
                    ))}
                    <span className="ml-1 text-sm text-gray-600">& up</span>
                  </div>
                </label>
              ))}
            </div>
          )}
        </div>

        {/* Availability */}
        <div>
          <button
            onClick={() => toggleSection('availability')}
            className="flex items-center justify-between w-full text-sm font-medium text-gray-700 mb-2"
          >
            Availability
            {expandedSections.availability ? (
              <ChevronUp className="w-4 h-4" />
            ) : (
              <ChevronDown className="w-4 h-4" />
            )}
          </button>
          
          {expandedSections.availability && (
            <div className="space-y-2">
              <label className="flex items-center">
                <input
                  type="checkbox"
                  checked={filters.inStock === true}
                  onChange={(e) => onFiltersChange({ 
                    inStock: e.target.checked ? true : undefined 
                  })}
                  className="mr-2 text-blue-600 focus:ring-blue-500"
                />
                <span className="text-sm text-gray-700">In Stock Only</span>
              </label>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
