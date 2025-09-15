import { Link } from 'react-router-dom';
import { Star, ShoppingCart, Heart } from 'lucide-react';
import { Product } from '@/types';
import { useCart } from '@/contexts/CartContext';
import { useState } from 'react';

interface ProductCardProps {
  product: Product;
  viewMode?: 'grid' | 'list';
}

export const ProductCard = ({ product, viewMode = 'grid' }: ProductCardProps) => {
  const { addItem, isLoading } = useCart();
  const [isWishlisted, setIsWishlisted] = useState(false);

  const handleAddToCart = async (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    
    try {
      await addItem(product, 1);
    } catch (error) {
      console.error('Failed to add to cart:', error);
    }
  };

  const handleWishlist = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsWishlisted(!isWishlisted);
  };

  if (viewMode === 'list') {
    return (
      <Link
        to={`/products/${product.id}`}
        className="flex items-center space-x-4 p-4 bg-white rounded-lg shadow-sm border hover:shadow-md transition-shadow"
      >
        <div className="w-24 h-24 bg-gray-100 rounded-lg overflow-hidden flex-shrink-0">
          <img
            src={product.imageUrl}
            alt={product.name}
            className="w-full h-full object-cover"
          />
        </div>
        
        <div className="flex-1 min-w-0">
          <h3 className="text-lg font-semibold text-gray-900 line-clamp-1">
            {product.name}
          </h3>
          <p className="text-gray-600 text-sm line-clamp-2 mt-1">
            {product.description}
          </p>
          
          <div className="flex items-center mt-2">
            <div className="flex items-center">
              {[...Array(5)].map((_, i) => (
                <Star
                  key={i}
                  className={`w-4 h-4 ${
                    i < Math.floor(product.rating)
                      ? 'text-yellow-400 fill-current'
                      : 'text-gray-300'
                  }`}
                />
              ))}
            </div>
            <span className="ml-2 text-sm text-gray-600">
              ({product.reviewCount})
            </span>
          </div>
        </div>
        
        <div className="text-right">
          <div className="text-xl font-bold text-gray-900">
            ${product.price.toFixed(2)}
          </div>
          {product.inStock ? (
            <span className="text-sm text-green-600">In Stock</span>
          ) : (
            <span className="text-sm text-red-600">Out of Stock</span>
          )}
        </div>
        
        <div className="flex flex-col space-y-2">
          <button
            onClick={handleAddToCart}
            disabled={!product.inStock || isLoading}
            className="p-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <ShoppingCart className="w-5 h-5" />
          </button>
          <button
            onClick={handleWishlist}
            className={`p-2 rounded-lg border ${
              isWishlisted
                ? 'bg-red-50 text-red-600 border-red-200'
                : 'bg-gray-50 text-gray-600 border-gray-200 hover:bg-gray-100'
            }`}
          >
            <Heart className={`w-5 h-5 ${isWishlisted ? 'fill-current' : ''}`} />
          </button>
        </div>
      </Link>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-sm border hover:shadow-md transition-shadow group">
      <Link to={`/products/${product.id}`} className="block">
        <div className="aspect-square bg-gray-100 rounded-t-lg overflow-hidden relative">
          <img
            src={product.imageUrl}
            alt={product.name}
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
          />
          
          {/* Wishlist button */}
          <button
            onClick={handleWishlist}
            className={`absolute top-3 right-3 p-2 rounded-full ${
              isWishlisted
                ? 'bg-red-100 text-red-600'
                : 'bg-white text-gray-600 hover:bg-gray-100'
            } shadow-sm opacity-0 group-hover:opacity-100 transition-opacity`}
          >
            <Heart className={`w-4 h-4 ${isWishlisted ? 'fill-current' : ''}`} />
          </button>
          
          {/* Stock badge */}
          {!product.inStock && (
            <div className="absolute top-3 left-3 px-2 py-1 bg-red-100 text-red-800 text-xs font-medium rounded-full">
              Out of Stock
            </div>
          )}
        </div>
        
        <div className="p-4">
          <h3 className="text-lg font-semibold text-gray-900 line-clamp-2 mb-2">
            {product.name}
          </h3>
          
          <p className="text-gray-600 text-sm line-clamp-2 mb-3">
            {product.description}
          </p>
          
          {/* Rating */}
          <div className="flex items-center mb-3">
            <div className="flex items-center">
              {[...Array(5)].map((_, i) => (
                <Star
                  key={i}
                  className={`w-4 h-4 ${
                    i < Math.floor(product.rating)
                      ? 'text-yellow-400 fill-current'
                      : 'text-gray-300'
                  }`}
                />
              ))}
            </div>
            <span className="ml-2 text-sm text-gray-600">
              ({product.reviewCount})
            </span>
          </div>
          
          {/* Price */}
          <div className="flex items-center justify-between">
            <div className="text-xl font-bold text-gray-900">
              ${product.price.toFixed(2)}
            </div>
            
            <button
              onClick={handleAddToCart}
              disabled={!product.inStock || isLoading}
              className="flex items-center px-3 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              <ShoppingCart className="w-4 h-4 mr-1" />
              Add
            </button>
          </div>
        </div>
      </Link>
    </div>
  );
};
