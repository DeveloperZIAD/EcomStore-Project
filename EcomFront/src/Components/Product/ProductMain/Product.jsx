// src/pages/Product.jsx (أو الملف اللي عندك)
import React, { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import { useDispatch } from "react-redux";
import { addToCart } from "../../../Features/Cart/cartSlice";

import api from "./../../../API/Services/api.js";

import { GoChevronLeft, GoChevronRight } from "react-icons/go";
import { FaStar } from "react-icons/fa";
import { FiHeart } from "react-icons/fi";
import { PiShareNetworkLight } from "react-icons/pi";
import toast from "react-hot-toast";

import "./Product.css";

const Product = () => {
  const { id } = useParams(); // جلب ID المنتج من الـ URL
  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [currentImg, setCurrentImg] = useState(0);
  const [quantity, setQuantity] = useState(1);
  const [clicked, setClicked] = useState(false);

  const dispatch = useDispatch();

  useEffect(() => {
    const fetchProduct = async () => {
      try {
        const res = await api.get(`/products/${id}`);
        setProduct(res.data);
      } catch (err) {
        setError("Failed to load product details.");
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchProduct();
  }, [id]);

  if (loading)
    return (
      <div className="text-center mt-5">
        <h3>Loading product...</h3>
      </div>
    );
  if (error)
    return <div className="alert alert-danger text-center mt-5">{error}</div>;
  if (!product)
    return (
      <div className="text-center mt-5">
        <h3>Product not found</h3>
      </div>
    );

  // الصور - لو في image_url واحد، أو array لو عايز متعدد
  const productImages = product.imageUrl ? [product.imageUrl] : []; // أو لو عندك array في الداتابيز

  const prevImg = () => {
    setCurrentImg(currentImg === 0 ? productImages.length - 1 : currentImg - 1);
  };

  const nextImg = () => {
    setCurrentImg(currentImg === productImages.length - 1 ? 0 : currentImg + 1);
  };

  const increment = () => setQuantity(quantity + 1);
  const decrement = () => quantity > 1 && setQuantity(quantity - 1);

  const handleAddToCart = () => {
    const productDetails = {
      productID: product.id,
      productName: product.name,
      productPrice: product.price,
      frontImg: product.imageUrl || "placeholder.jpg",
      quantity: quantity,
    };

    dispatch(addToCart(productDetails));
    toast.success(`${quantity} x ${product.name} added to cart!`, {
      duration: 2000,
    });
  };

  const handleWishClick = () => setClicked(!clicked);

  return (
    <>
      <div className="productSection">
        <div className="productShowCase">
          <div className="productGallery">
            <div className="productThumb">
              {productImages.map((img, index) => (
                <img
                  key={index}
                  src={img}
                  onClick={() => setCurrentImg(index)}
                  alt={`Thumbnail ${index + 1}`}
                />
              ))}
            </div>
            <div className="productFullImg">
              <img
                src={productImages[currentImg] || "placeholder.jpg"}
                alt={product.name}
              />
              {productImages.length > 1 && (
                <div className="buttonsGroup">
                  <button onClick={prevImg} className="directionBtn">
                    <GoChevronLeft size={18} />
                  </button>
                  <button onClick={nextImg} className="directionBtn">
                    <GoChevronRight size={18} />
                  </button>
                </div>
              )}
            </div>
          </div>

          <div className="productDetails">
            <div className="productBreadcrumb">
              <div className="breadcrumbLink">
                <Link to="/">Home</Link>&nbsp;/&nbsp;
                <Link to="/shop">Shop</Link>&nbsp;/&nbsp;
                {product.name}
              </div>
            </div>

            <div className="productName">
              <h1>{product.name}</h1>
            </div>

            <div className="productRating">
              {/* لو عندك rating في الداتابيز */}
              <FaStar color="#FEC78A" size={10} />
              <FaStar color="#FEC78A" size={10} />
              <FaStar color="#FEC78A" size={10} />
              <FaStar color="#FEC78A" size={10} />
              <FaStar color="#FEC78A" size={10} />
              <p>{product.stock > 0 ? "In stock" : "Out of stock"}</p>
            </div>

            <div className="productPrice">
              <h3>${product.price}</h3>
            </div>

            <div className="productDescription">
              <p>{product.description || "No description available."}</p>
            </div>

            <div className="productCartQuantity">
              <div className="productQuantity">
                <button onClick={decrement}>-</button>
                <input type="text" value={quantity} readOnly />
                <button onClick={increment}>+</button>
              </div>
              <div className="productCartBtn">
                <button
                  onClick={handleAddToCart}
                  disabled={product.stock === 0}
                >
                  {product.stock === 0 ? "Out of Stock" : "Add to Cart"}
                </button>
              </div>
            </div>

            <div className="productWishShare">
              <div className="productWishList">
                <button onClick={handleWishClick}>
                  <FiHeart color={clicked ? "red" : ""} size={17} />
                  <p>Add to Wishlist</p>
                </button>
              </div>
              <div className="productShare">
                <PiShareNetworkLight size={22} />
                <p>Share</p>
              </div>
            </div>

            <div className="productTags">
              <p>
                <span>Category: </span>
                {product.categoryName || "Uncategorized"}
              </p>
              <p>
                <span>Stock: </span>
                {product.stock}
              </p>
            </div>
          </div>
        </div>
      </div>
    </>
  );
};

export default Product;
