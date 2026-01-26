// src/pages/ShoppingCart.jsx
import React, { useState } from "react";
import "./ShoppingCart.css";

import { useSelector, useDispatch } from "react-redux";
import {
  removeFromCart,
  updateQuantity,
  selectCartTotalAmount,
  clearCart,
} from "../../Features/Cart/cartSlice";

import { MdOutlineClose } from "react-icons/md";

import { Link } from "react-router-dom";

import success from "../../Assets/success.png";

import api from "../../API/Services/api.js";
import toast from "react-hot-toast";

const ShoppingCart = () => {
  const cartItems = useSelector((state) => state.cart.items);
  const dispatch = useDispatch();

  const [activeTab, setActiveTab] = useState("cartTab1");
  const [payments, setPayments] = useState(false);
  const [orderData, setOrderData] = useState({});

  const totalPrice = useSelector(selectCartTotalAmount);

  const handleTabClick = (tab) => {
    if (tab === "cartTab1" || cartItems.length > 0) {
      setActiveTab(tab);
    }
  };

  const handleQuantityChange = (productId, quantity) => {
    if (quantity >= 1 && quantity <= 20) {
      dispatch(updateQuantity({ productID: productId, quantity: quantity }));
    }
  };

  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const currentDate = new Date();
  const formatDate = (date) => {
    const day = String(date.getDate()).padStart(2, "0");
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
  };

  const orderNumber = Math.floor(Math.random() * 100000);

  const [selectedPayment, setSelectedPayment] = useState("Cash on delivery");

  const handlePaymentChange = (e) => {
    setSelectedPayment(e.target.value);
  };

  const [checkoutForm, setCheckoutForm] = useState({
    email: "",
    username: "",
    street: "",
    city: "",
    state: "",
    country: "",
    zip_code: "",
  });

  const handleCheckoutChange = (e) => {
    setCheckoutForm({ ...checkoutForm, [e.target.name]: e.target.value });
  };

  const handlePlaceOrder = async () => {
    if (cartItems.length === 0) {
      toast.error("Cart is empty!");
      return;
    }

    // Validate required fields
    if (
      !checkoutForm.email ||
      !checkoutForm.street ||
      !checkoutForm.city ||
      !checkoutForm.country ||
      !checkoutForm.zip_code
    ) {
      toast.error("Please fill all required billing fields.");
      return;
    }

    const items = cartItems.map((item) => ({
      product_id: item.productID,
      quantity: item.quantity,
      price_at_purchase: item.productPrice,
    }));
    let username = Math.random().toString(36).substring(2, 10); // Generate a random username
    const orderPayload = {
      email: checkoutForm.email,
      username: checkoutForm.username || username, // مطلوب, خليه "guest" لو فاضي
      street: checkoutForm.street,
      city: checkoutForm.city,
      state: checkoutForm.state || null,
      country: checkoutForm.country,
      zipCode: checkoutForm.zip_code,
      items: items,
      paymentMethod: selectedPayment,
      paymentStatus: "pending",
      transactionId: "test-transaction-" + Date.now(), // مطلوب, خليه قيمة مؤقتة
    };

    console.log("Sending order payload:", orderPayload);

    try {
      const res = await api.post("/guestcheckout", orderPayload);
      console.log("Order response:", res.data);
      if (res.data.orderId || res.data.Success) {
        setOrderData(res.data);
        dispatch(clearCart());
        handleTabClick("cartTab3");
        setPayments(true);
        toast.success(
          `Order placed successfully! Order ID: ${res.data.orderId || "N/A"}`
        );
      }
    } catch (err) {
      toast.error("Failed to place order. Check console.");
      console.error("Order error:", err.response ? err.response.data : err);
    }
  };

  return (
    <>
      <div className="shoppingCartSection">
        <h2>Cart</h2>

        <div className="shoppingCartTabsContainer">
          <div className={`shoppingCartTabs ${activeTab}`}>
            {/* Tab buttons - your existing code */}
            <button
              className={activeTab === "cartTab1" ? "active" : ""}
              onClick={() => {
                handleTabClick("cartTab1");
                setPayments(false);
              }}
            >
              <div className="shoppingCartTabsNumber">
                <h3>01</h3>
                <div className="shoppingCartTabsHeading">
                  <h3>Shopping Bag</h3>
                  <p>Manage Your Items List</p>
                </div>
              </div>
            </button>
            <button
              className={activeTab === "cartTab2" ? "active" : ""}
              onClick={() => {
                handleTabClick("cartTab2");
                setPayments(false);
              }}
              disabled={cartItems.length === 0}
            >
              <div className="shoppingCartTabsNumber">
                <h3>02</h3>
                <div className="shoppingCartTabsHeading">
                  <h3>Shipping and Checkout</h3>
                  <p>Checkout Your Items List</p>
                </div>
              </div>
            </button>
            <button
              className={activeTab === "cartTab3" ? "active" : ""}
              onClick={() => handleTabClick("cartTab3")}
              disabled={cartItems.length === 0 || payments === false}
            >
              <div className="shoppingCartTabsNumber">
                <h3>03</h3>
                <div className="shoppingCartTabsHeading">
                  <h3>Confirmation</h3>
                  <p>Review And Submit Your Order</p>
                </div>
              </div>
            </button>
          </div>

          <div className="shoppingCartTabsContent">
            {/* Tab 1: Shopping Bag */}
            {activeTab === "cartTab1" && (
              <div className="shoppingBagSection">
                {/* Your existing table code for cart items */}
                {/* Cart Totals */}
                <div className="shoppingBagTotal">
                  <h3>Cart Totals</h3>
                  <table className="shoppingBagTotalTable">
                    <tbody>
                      <tr>
                        <th>Subtotal</th>
                        <td>${totalPrice.toFixed(2)}</td>
                      </tr>
                      <tr>
                        <th>Shipping</th>
                        <td>$5.00</td>
                      </tr>
                      <tr>
                        <th>VAT</th>
                        <td>$11.00</td>
                      </tr>
                      <tr>
                        <th>Total</th>
                        <td>${(totalPrice + 16).toFixed(2)}</td>
                      </tr>
                    </tbody>
                  </table>
                  <button
                    onClick={() => {
                      handleTabClick("cartTab2");
                      scrollToTop();
                    }}
                    disabled={cartItems.length === 0}
                  >
                    Proceed to Checkout
                  </button>
                </div>
              </div>
            )}

            {/* Tab 2: Checkout */}
            {activeTab === "cartTab2" && (
              <div className="checkoutSection">
                <div className="checkoutDetailsSection">
                  <h4>Billing Details</h4>
                  <form>
                    <div className="checkoutDetailsFormRow">
                      <input type="text" placeholder="First Name" required />
                      <input type="text" placeholder="Last Name" required />
                    </div>
                    <input
                      type="email"
                      name="email"
                      placeholder="Email *"
                      onChange={handleCheckoutChange}
                      required
                    />
                    <input
                      type="text"
                      name="username"
                      placeholder="Username (optional)"
                      onChange={handleCheckoutChange}
                    />
                    <input
                      type="text"
                      name="street"
                      placeholder="Street Address *"
                      onChange={handleCheckoutChange}
                      required
                    />
                    <input
                      type="text"
                      name="city"
                      placeholder="Town / City *"
                      onChange={handleCheckoutChange}
                      required
                    />
                    <input
                      type="text"
                      name="state"
                      placeholder="State (optional)"
                      onChange={handleCheckoutChange}
                    />
                    <input
                      type="text"
                      name="country"
                      placeholder="Country *"
                      onChange={handleCheckoutChange}
                      required
                    />
                    <input
                      type="text"
                      name="zip_code"
                      placeholder="Postcode / ZIP *"
                      onChange={handleCheckoutChange}
                      required
                    />
                  </form>
                </div>

                <div className="checkoutPaymentSection">
                  <h4>Payment Method</h4>
                  <div className="paymentMethods">
                    <label className="paymentOption">
                      <input
                        type="radio"
                        name="payment"
                        value="cash_on_delivery"
                        checked={selectedPayment === "cash_on_delivery"}
                        onChange={handlePaymentChange}
                      />
                      <div className="paymentDetails">
                        <span className="paymentTitle">Cash on Delivery</span>
                        <p>Pay with cash upon delivery.</p>
                      </div>
                    </label>

                    <label className="paymentOption">
                      <input
                        type="radio"
                        name="payment"
                        value="bank_transfer"
                        checked={selectedPayment === "bank_transfer"}
                        onChange={handlePaymentChange}
                      />
                      <div className="paymentDetails">
                        <span className="paymentTitle">
                          Direct Bank Transfer
                        </span>
                        <p>Make your payment directly into our bank account.</p>
                      </div>
                    </label>

                    <label className="paymentOption">
                      <input
                        type="radio"
                        name="payment"
                        value="paypal"
                        checked={selectedPayment === "paypal"}
                        onChange={handlePaymentChange}
                      />
                      <div className="paymentDetails">
                        <span className="paymentTitle">PayPal</span>
                        <p>Pay via PayPal.</p>
                      </div>
                    </label>

                    <label className="paymentOption">
                      <input
                        type="radio"
                        name="payment"
                        value="credit_card"
                        checked={selectedPayment === "credit_card"}
                        onChange={handlePaymentChange}
                      />
                      <div className="paymentDetails">
                        <span className="paymentTitle">
                          Credit Card (Test Mode)
                        </span>
                        <p>Use test card: 4242 4242 4242 4242</p>
                      </div>
                    </label>
                  </div>

                  <div className="policyText mt-4">
                    Your personal data will be used to process your order and
                    for other purposes described in our privacy policy.
                  </div>

                  <button
                    onClick={handlePlaceOrder}
                    className="placeOrderBtn mt-4"
                  >
                    Place Order
                  </button>
                </div>
              </div>
            )}

            {/* Tab 3: Confirmation */}
            {activeTab === "cartTab3" && (
              <div className="orderCompleteSection">
                <div className="orderComplete">
                  <div className="orderCompleteMessage">
                    <div className="orderCompleteMessageImg">
                      <img src={success} alt="Success" />
                    </div>
                    <h3>Your order is completed!</h3>
                    <p>Thank you. Your order has been received.</p>
                  </div>
                  <div className="orderInfo">
                    <div className="orderInfoItem">
                      <p>Order Number</p>
                      <h4>{orderData.orderId || orderNumber}</h4>
                    </div>
                    <div className="orderInfoItem">
                      <p>Date</p>
                      <h4>{formatDate(currentDate)}</h4>
                    </div>
                    <div className="orderInfoItem">
                      <p>Total</p>
                      <h4>${(totalPrice + 16).toFixed(2)}</h4>
                    </div>
                    <div className="orderInfoItem">
                      <p>Payment Method</p>
                      <h4>{selectedPayment.replace("_", " ").toUpperCase()}</h4>
                    </div>
                  </div>

                  {/* Order Details */}
                <div className="orderTotalContainer">
  <h3>Order Details</h3>

 

  {/* Order Info */}
  <div className="orderInfo mt-4">
    <div className="orderInfoItem">
      <p>Order Number</p>
      <h4>#{orderData.orderId || orderNumber}</h4>
    </div>
    <div className="orderInfoItem">
      <p>Date</p>
      <h4>{formatDate(currentDate)}</h4>
    </div>
    <div className="orderInfoItem">
      <p>Payment Method</p>
      <h4>{selectedPayment.replace("_", " ").toUpperCase()}</h4>
    </div>
  </div>
</div>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </>
  );
};

export default ShoppingCart;
