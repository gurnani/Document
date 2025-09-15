import { useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { CheckCircle, Package, ArrowRight } from 'lucide-react';

export const OrderSuccess = () => {
  const location = useLocation();
  const orderId = location.state?.orderId;

  useEffect(() => {
    localStorage.removeItem('guestCart');
  }, []);

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full">
        <div className="bg-white rounded-lg shadow-lg p-8 text-center">
          <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-6">
            <CheckCircle className="w-10 h-10 text-green-600" />
          </div>

          <h1 className="text-2xl font-bold text-gray-900 mb-2">
            Order Confirmed!
          </h1>
          
          <p className="text-gray-600 mb-6">
            Thank you for your purchase. Your order has been successfully placed and is being processed.
          </p>

          {orderId && (
            <div className="bg-gray-50 rounded-lg p-4 mb-6">
              <p className="text-sm text-gray-600 mb-1">Order Number</p>
              <p className="text-lg font-semibold text-gray-900">#{orderId}</p>
            </div>
          )}

          <div className="space-y-4 mb-8">
            <div className="flex items-center text-sm text-gray-600">
              <div className="w-2 h-2 bg-green-500 rounded-full mr-3"></div>
              Order confirmation email sent
            </div>
            <div className="flex items-center text-sm text-gray-600">
              <div className="w-2 h-2 bg-green-500 rounded-full mr-3"></div>
              Payment processed successfully
            </div>
            <div className="flex items-center text-sm text-gray-600">
              <div className="w-2 h-2 bg-yellow-500 rounded-full mr-3"></div>
              Preparing your order for shipment
            </div>
          </div>

          <div className="space-y-3">
            <Link
              to="/profile?tab=orders"
              className="w-full flex items-center justify-center px-4 py-2 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition-colors"
            >
              <Package className="w-4 h-4 mr-2" />
              Track Your Order
            </Link>
            
            <Link
              to="/products"
              className="w-full flex items-center justify-center px-4 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition-colors"
            >
              Continue Shopping
              <ArrowRight className="w-4 h-4 ml-2" />
            </Link>
          </div>

          <div className="mt-8 pt-6 border-t border-gray-200">
            <h3 className="text-sm font-medium text-gray-900 mb-3">
              What happens next?
            </h3>
            <div className="text-sm text-gray-600 space-y-2">
              <p>1. We'll send you a confirmation email with order details</p>
              <p>2. Your order will be processed within 1-2 business days</p>
              <p>3. You'll receive tracking information once shipped</p>
              <p>4. Estimated delivery: 3-5 business days</p>
            </div>
          </div>

          <div className="mt-6 pt-4 border-t border-gray-200">
            <p className="text-xs text-gray-500">
              Need help? Contact our{' '}
              <Link to="/support" className="text-blue-600 hover:text-blue-700">
                customer support team
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};
