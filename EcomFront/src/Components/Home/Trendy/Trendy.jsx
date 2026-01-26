// src/components/Trendy.jsx أو الملف اللي عندك
import React, { useState, useEffect } from "react";
import "./Trendy.css";
import { useDispatch } from "react-redux";
import { addToCart } from "../../../Features/Cart/cartSlice";
import { Link } from "react-router-dom";
import api from './../../../API/Services/api.js';

import { FiHeart } from "react-icons/fi";
import { FaStar, FaCartPlus } from "react-icons/fa";
import toast from "react-hot-toast";

const Trendy = () => {
  const dispatch = useDispatch();
  const [activeTab, setActiveTab] = useState("new");
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [wishList, setWishList] = useState({});

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        const res = await api.get("/products");
        setProducts(res.data);
      } catch (err) {
        console.error("Failed to load products:", err);
        toast.error("Failed to load products");
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, []);

  const handleWishlistClick = (productId) => {
    setWishList((prev) => ({
      ...prev,
      [productId]: !prev[productId],
    }));
  };

  const handleAddToCart = (product) => {
    const productDetails = {
      productID: product.id,
      productName: product.name,
      productPrice: product.price,
      frontImg: product.image_url || "placeholder.jpg",
      quantity: 1,
    };

    dispatch(addToCart(productDetails));
    toast.success(`${product.name} added to cart!`);
  };

  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  // Sorting functions
  const getSortedProducts = () => {
    let sorted = [...products];

    switch (activeTab) {
      case "new":
        // افترض إن عندك created_at في الداتابيز، أو نعمل random للتجربة
        return sorted.sort((a, b) => b.id - a.id); // أحدث حسب ID
      case "featured":
        return sorted; // أو لو عندك is_featured field
      case "topRated":
        // لو عندك rating field، أو نعمل random للتجربة
        return sorted.sort((a, b) => b.price - a.price); // مثال
      case "lowPrice":
        return sorted.sort((a, b) => a.price - b.price);
      default:
        return sorted;
    }
  };

  const displayedProducts = getSortedProducts().slice(0, 8);

  if (loading) return <div className="text-center mt-5"><h3>Loading Trendy Products...</h3></div>;

  return (
    <>
      <div className="trendyProducts">
        <h2>
          Our Trendy <span>Products</span>
        </h2>

        <div className="trendyMainContainer">
          {displayedProducts.map((product) => (
            <div className="trendyProductContainer" key={product.id}>
              <div className="trendyProductImages">
                <Link to={`/product/${product.id}`} onClick={scrollToTop}>
                  <img src={product.imageUrl || "placeholder.jpg"} alt={product.name} className="trendyProduct_front" />
                  {/* لو عايز back image، أضف field في الداتابيز */}
                </Link>
                <h4 onClick={() => handleAddToCart(product)}>Add to Cart</h4>
              </div>

              <div className="trendyProductImagesCart" onClick={() => handleAddToCart(product)}>
                <FaCartPlus />
              </div>

              <div className="trendyProductInfo">
                <div className="trendyProductCategoryWishlist">
                  <p>{product.categoryName || "Category"}</p>
                  <FiHeart
                    onClick={() => handleWishlistClick(product.id)}
                    style={{ color: wishList[product.id] ? "red" : "#767676", cursor: "pointer" }}
                  />
                </div>

                <div className="trendyProductNameInfo">
                  <Link to={`/product/${product.id}`} onClick={scrollToTop}>
                    <h5>{product.name}</h5>
                  </Link>
                  <p>${product.price}</p>
                  <div className="trendyProductRatingReviews">
                    <div className="trendyProductRatingStar">
                      <FaStar color="#FEC78A" size={10} />
                      <FaStar color="#FEC78A" size={10} />
                      <FaStar color="#FEC78A" size={10} />
                      <FaStar color="#FEC78A" size={10} />
                      <FaStar color="#FEC78A" size={10} />
                    </div>
                    <span>{product.stock > 0 ? "In stock" : "Out of stock"}</span>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </>
  );
};

export default Trendy;