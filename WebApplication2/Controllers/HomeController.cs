﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebApplication2.Data;
using WebApplication2.Models;
using WebApplication2.Services;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Identity;
using WebApplication2.ViewModels;
using System.Security.Claims;
using WebApplication2.Repositories;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Dummy lists of products for shopping cart
        private List<Product> products = new List<Product>();
        private List<Product> purchasedItems = new List<Product>();

        // Services
        private ProductService _productService;
        private OrderService _orderService;
        private OrderProductService _orderProductService;

        // Repositories from generic repository class
        private IGenericRepository<Order> _orderRepository = null;
        private IGenericRepository<Product> _productRepository = null;
        private IGenericRepository<OrderProduct> _orderProductRepository = null;

        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;

            // Repositories
            _orderRepository = new GenericRepository<Order>();
            _productRepository = new GenericRepository<Product>();
            _orderProductRepository = new GenericRepository<OrderProduct>();

            _userManager = userManager;

            // Services
            _productService = new ProductService();
            _orderService = new OrderService();
            _orderProductService = new OrderProductService();
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Product(int id)
        {

            return View(_productRepository.GetById(id));
        }

        public IActionResult Products(string category)
        {

            return View(_productService.SortByCategory(category));
        }

        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Colors()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ShoppingCartAsync()
        {
            ViewBag.Purshases = purchasedItems;
            ViewBag.CurrentUser = await _userManager.GetUserAsync(User);
            List<Product> productList = new List<Product>();
            productList.Add(_productService.GetProductById(1));
            productList.Add(_productService.GetProductById(2));
            ViewBag.Products = productList;
            List<Order> orders = _orderService.GetAll();
            
            List<Product> products = ViewBag.Products;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ShoppingCartAsync(OrderViewModel cartOrder, List<int> shoppingCartIds, List<int> shoppingCartQuantities)
        {
            //_productService.Create(product);
            //cartOrder.OrderProduct = new OrderProduct() { Quantity = cartOrder.OrderProduct.Quantity };
            cartOrder.Order = new Order();
            //cartOrder.Order.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _orderService.Create(cartOrder.Order);
            cartOrder.OrderProduct = new OrderProduct();
            cartOrder.OrderProduct.OrderId = cartOrder.Order.Id;
            for(int i = 1; i <= shoppingCartIds.Count; i++)
            {
                cartOrder.OrderProduct.ProductId = shoppingCartIds[i-1];
                cartOrder.OrderProduct.Quantity = shoppingCartQuantities[i - 1];
                _orderProductService.Create(cartOrder.OrderProduct);
                cartOrder.OrderProduct.Id = 0;
            }
            //cartOrder.OrderProduct.ProductId = shoppingCartIds[0];
            //_orderProductService.Create(cartOrder.OrderProduct);
            //For populating the view after order is made
            ViewBag.CurrentUser = await _userManager.GetUserAsync(User);
            List<Product> productList = new List<Product>();
            productList.Add(_productService.GetProductById(1));
            productList.Add(_productService.GetProductById(2));
            ViewBag.Products = productList;
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Search(string result)
        {
            var product = _productService.SearchProducts(result);

            if (product != null)
            {
                var resultCount = product.Count();

                ViewBag.searchResult = $"Visar {resultCount} resultat som matchar <b>{result}</b>.";

                return View(await product.ToListAsync());
            }

            ViewBag.searchResult = $"Inga resultat matchar <b>{result}</b>.";

            return View(new List<Product>());
        }
    }
}
