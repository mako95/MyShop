using System;
using System.Linq;
using System.Security.Principal;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyShop.Core.Contracts;
using MyShop.Core.Models;
using MyShop.Core.ViewModels;
using MyShop.Services;
using MyShop.WebUI.Controllers;
using MyShop.WebUI.Tests.Mocks;

namespace MyShop.WebUI.Tests.Controllers
{
    [TestClass]
    public class BasketControllerTests
    {
        [TestMethod]
        public void CanAddBasketItem()
        {
            ///Arrange
            IRepository<Basket> basketContext = new MockContext<Basket>();
            IRepository<Product> productContext = new MockContext<Product>();
            IRepository<Order> orderContext = new MockContext<Order>();
            IRepository<Customer> customerContext = new MockContext<Customer>();
            MockHttpContext httpContext = new MockHttpContext();
            
            IBasketService basketService = new BasketService(productContext, basketContext);
            IOrderService orderService = new OrderService(orderContext);
            BasketController controller = new BasketController(basketService, orderService, customerContext);
            controller.ControllerContext = new System.Web.Mvc.ControllerContext(httpContext, new System.Web.Routing.RouteData(), controller);

            ///Act
            //basketService.AddToBasket(httpContext, "1");
            controller.AddToBasket("1");
            Basket basket = basketContext.Collection().FirstOrDefault();

            ///Assert
            Assert.IsNotNull(basket);
            Assert.AreEqual(1, basket.BasketItems.Count());
            Assert.AreEqual("1", basket.BasketItems.FirstOrDefault().ProductId);
        }

        [TestMethod]
        public void CanGetSummaryViewModel()
        {
            ///Arrange
            IRepository<Basket> basketContext = new MockContext<Basket>();
            IRepository<Product> productContext = new MockContext<Product>();
            IRepository<Order> orderContext = new MockContext<Order>();
            IRepository<Customer> customerContext = new MockContext<Customer>();
            MockHttpContext httpContext = new MockHttpContext();
            Basket basket = new Basket();

            IBasketService basketService = new BasketService(productContext, basketContext);
            IOrderService orderService = new OrderService(orderContext);
            BasketController controller = new BasketController(basketService, orderService, customerContext);
            controller.ControllerContext = new System.Web.Mvc.ControllerContext(httpContext, new System.Web.Routing.RouteData(), controller);
            httpContext.Request.Cookies.Add(new System.Web.HttpCookie("eCommerceBasket") { Value = basket.Id });

            ///Act
            productContext.Insert(new Product() { Id = "1", Price = 5.00m });
            productContext.Insert(new Product() { Id = "2", Price = 10.00m });

            basket.BasketItems.Add(new BasketItem() { ProductId = "1", Quantity = 1 });
            basket.BasketItems.Add(new BasketItem() { ProductId = "2", Quantity = 2 });

            basketContext.Insert(basket);

            var result = controller.BasketSummary() as PartialViewResult;
            var basketSummary = (BasketSummaryViewModel)result.ViewData.Model;

            ///Assert
            Assert.AreEqual(3, basketSummary.BasketCount);
            Assert.AreEqual(25, basketSummary.BasketTotal);
        }

        [TestMethod]
        public void CanCheckoutAndCreateOrder()
        {
            //Arrange
            IRepository<Product> productContext = new MockContext<Product>();
            productContext.Insert(new Product() { Id = "1", Price = 10.00m });
            productContext.Insert(new Product() { Id = "2", Price = 5.00m });

            IRepository<Basket> basketContext = new MockContext<Basket>();
            Basket basket = new Basket();
            basket.BasketItems.Add(new BasketItem() { ProductId = "1", Quantity = 2, BasketId = basket.Id });
            basket.BasketItems.Add(new BasketItem() { ProductId = "2", Quantity = 1, BasketId = basket.Id });
            basketContext.Insert(basket);
            
            IRepository<Order> orderContext = new MockContext<Order>();

            IRepository<Customer> customerContext = new MockContext<Customer>();

            IBasketService basketService = new BasketService(productContext, basketContext);
            IOrderService orderService = new OrderService(orderContext);

            customerContext.Insert(new Customer() { Id = "1", Email = "mako.2@hotmail.com", ZipCode = "12345" });
            IPrincipal FakeUser = new GenericPrincipal(new GenericIdentity("mako.2@hotmail.com", "Forms"), null);

            BasketController basketController = new BasketController(basketService, orderService, customerContext);
            var httpContext = new MockHttpContext();
            httpContext.User = FakeUser;
            httpContext.Request.Cookies.Add(new System.Web.HttpCookie("eCommerceBasket") { Value = basket.Id });
            basketController.ControllerContext = new ControllerContext(httpContext, new System.Web.Routing.RouteData(), basketController);

            //Act
            Order order = new Order();
            basketController.Checkout(order);

            Order orderInRep = orderContext.Find(order.Id);

            //Assert
            Assert.AreEqual(2, order.OrderItems.Count);
            Assert.AreEqual(0, basket.BasketItems.Count);
            Assert.AreEqual(2, orderInRep.OrderItems.Count);
        }
    }
}
