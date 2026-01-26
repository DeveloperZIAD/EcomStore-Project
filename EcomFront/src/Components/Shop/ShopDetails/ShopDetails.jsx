// src/pages/ShopDetails.jsx (أو المسار اللي عندك)
import React, { useState, useEffect } from "react";
import "./ShopDetails.css";

import { useDispatch } from "react-redux";
import { addToCart } from "../../../Features/Cart/cartSlice";

import { Link } from "react-router-dom";
import api from './../../../API/Services/api.js';
import { FiHeart } from "react-icons/fi";
import { FaStar } from "react-icons/fa";
import { FaAngleRight, FaAngleLeft } from "react-icons/fa6";
import { FaCartPlus } from "react-icons/fa";
import toast from "react-hot-toast";

const ShopDetails = () => {
  const dispatch = useDispatch();
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [wishList, setWishList] = useState({});

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        const res = await api.get("/products");
        setProducts(res.data);
        console.log("Products from API:", res.data);
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

  if (loading) return <div className="text-center mt-5"><h3>Loading products...</h3></div>;

  // عرض أول 6 منتجات (أو كل اللي موجود)
  const displayedProducts = products.slice(0, 6);

  return (
    <>
      <div className="shopDetails">
        <div className="shopDetailMain">
          <div className="shopDetails__left">
            {/* Sidebar لو موجود، هنعدله لاحقًا لو عايز */}
          </div>

          <div className="shopDetails__right">
            <div className="shopDetailsSorting">
              <div className="shopDetailsBreadcrumbLink">
                <Link to="/" onClick={scrollToTop}>Home</Link>&nbsp;/&nbsp;
                <Link to="/shop">The Shop</Link>
              </div>
            </div>

            <div className="shopDetailsProducts">
              <div className="shopDetailsProductsContainer">
                {displayedProducts.map((product) => (
                  <div className="sdProductContainer" key={product.id}>
                    <div className="sdProductImages">
                      <Link to={`/product/${product.id}`} onClick={scrollToTop}>
                        <img
                          src={product.imageUrl || "placeholder.jpg"}
                          alt={product.name}
                          className="sdProduct_front"
                        />
                        {/* لو عايز back image، أضف field في الداتابيز */}
                      </Link>
                      <h4 onClick={() => handleAddToCart(product)}>
                        Add to Cart
                      </h4>
                    </div>

                    <div className="sdProductImagesCart" onClick={() => handleAddToCart(product)}>
                      <FaCartPlus />
                    </div>

                    <div className="sdProductInfo">
                      <div className="sdProductCategoryWishlist">
                        <p>{product.categoryName || "Category"}</p>
                        <FiHeart
                          onClick={() => handleWishlistClick(product.id)}
                          style={{
                            color: wishList[product.id] ? "red" : "#767676",
                            cursor: "pointer",
                          }}
                        />
                      </div>

                      <div className="sdProductNameInfo">
                        <Link to={`/product/${product.id}`} onClick={scrollToTop}>
                          <h5>{product.name}</h5>
                        </Link>
                        <p>${product.price}</p>
                        <div className="sdProductRatingReviews">
                          <div className="sdProductRatingStar">
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

            {/* Pagination (مؤقت، لو عايز pagination حقيقي نعملها لاحقًا) */}
            <div className="shopDetailsPagination">
              <div className="sdPaginationPrev">
                <p onClick={scrollToTop}>
                  <FaAngleLeft />
                  Prev
                </p>
              </div>
              <div className="sdPaginationNumber">
                <div className="paginationNum">
                  <p onClick={scrollToTop}>1</p>
                  <p onClick={scrollToTop}>2</p>
                </div>
              </div>
              <div className="sdPaginationNext">
                <p onClick={scrollToTop}>
                  Next
                  <FaAngleRight />
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </>
  );
};

export default ShopDetails;