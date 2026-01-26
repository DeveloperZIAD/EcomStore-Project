// src/pages/admin/Dashboard.jsx
import React, { useState, useEffect } from "react";
import api from "../../API/Services/api.js";
import "./index.css"; // استيراد الـ CSS

const AdminDashboard = () => {
  const [stats, setStats] = useState({
    totalProducts: 0,
    totalCategories: 0,
    totalOrders: 0,
    totalUsers: 0,
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const [productsRes, categoriesRes, ordersRes, usersRes] =
          await Promise.all([
            api.get("/products"),
            api.get("/categories"),
            api.get("/orders"), // Admin only endpoint
            api.get("/users"), // Admin only endpoint
          ]);

        setStats({
          totalProducts: productsRes.data.length,
          totalCategories: categoriesRes.data.length,
          totalOrders: ordersRes.data.length,
          totalUsers: usersRes.data.length,
        });
        console.log("Dashboard stats fetched successfully.");
      } catch (err) {
        setError("Failed to load dashboard data.");
        console.error("Failed to load dashboard data:", err);
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, []);

  if (loading) {
    return (
      <div className="loading-container">
        <h3>Loading Dashboard...</h3>
      </div>
    );
  }

  if (error) {
    return <div className="alert alert-danger error-alert">{error}</div>;
  }

  return (
    <div className="admin-dashboard">
      <h2 className="dashboard-title">Admin Dashboard</h2>

      <div className="row">
        {/* Total Products */}
        <div className="col-xl-3 col-md-6 mb-4">
          <div className="card border-left-primary shadow h-100 py-2 stats-card">
            <div className="card-body">
              <div className="row no-gutters align-items-center">
                <div className="col mr-2">
                  <div className="text-xs font-weight-bold text-primary text-uppercase mb-1 card-title">
                    Total Products
                  </div>
                  <div className="h5 mb-0 font-weight-bold text-gray-800 card-value">
                    {stats.totalProducts}
                  </div>
                </div>
                <div className="col-auto">
                  <i className="fas fa-box fa-2x text-gray-300 card-icon"></i>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Total Categories */}
        <div className="col-xl-3 col-md-6 mb-4">
          <div className="card border-left-success shadow h-100 py-2 stats-card">
            <div className="card-body">
              <div className="row no-gutters align-items-center">
                <div className="col mr-2">
                  <div className="text-xs font-weight-bold text-success text-uppercase mb-1 card-title">
                    Total Categories
                  </div>
                  <div className="h5 mb-0 font-weight-bold text-gray-800 card-value">
                    {stats.totalCategories}
                  </div>
                </div>
                <div className="col-auto">
                  <i className="fas fa-list fa-2x text-gray-300 card-icon"></i>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Total Orders */}
        <div className="col-xl-3 col-md-6 mb-4">
          <div className="card border-left-info shadow h-100 py-2 stats-card">
            <div className="card-body">
              <div className="row no-gutters align-items-center">
                <div className="col mr-2">
                  <div className="text-xs font-weight-bold text-info text-uppercase mb-1 card-title">
                    Total Orders
                  </div>
                  <div className="h5 mb-0 font-weight-bold text-gray-800 card-value">
                    {stats.totalOrders}
                  </div>
                </div>
                <div className="col-auto">
                  <i className="fas fa-shopping-cart fa-2x text-gray-300 card-icon"></i>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Total Users */}
        <div className="col-xl-3 col-md-6 mb-4">
          <div className="card border-left-warning shadow h-100 py-2 stats-card">
            <div className="card-body">
              <div className="row no-gutters align-items-center">
                <div className="col mr-2">
                  <div className="text-xs font-weight-bold text-warning text-uppercase mb-1 card-title">
                    Total Users
                  </div>
                  <div className="h5 mb-0 font-weight-bold text-gray-800 card-value">
                    {stats.totalUsers}
                  </div>
                </div>
                <div className="col-auto">
                  <i className="fas fa-users fa-2x text-gray-300 card-icon"></i>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <h4 className="quick-actions-title">Quick Actions</h4>
      <div className="row">
        <div className="col-md-4 mb-3">
          <a
            href="/admin/products"
            className="btn btn-primary btn-lg btn-block quick-action-btn"
          >
            Manage Products
          </a>
        </div>
        <div className="col-md-4 mb-3">
          <a
            href="/admin/categories"
            className="btn btn-success btn-lg btn-block quick-action-btn"
          >
            Manage Categories
          </a>
        </div>
        <div className="col-md-4 mb-3">
          <a
            href="/admin/orders"
            className="btn btn-info btn-lg btn-block quick-action-btn"
          >
            View Orders
          </a>
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard;
