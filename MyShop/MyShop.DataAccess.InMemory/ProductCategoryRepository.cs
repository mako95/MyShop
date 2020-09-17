using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.DataAccess.InMemory
{
    public class ProductCategoryRepository
    {
        ObjectCache cache = MemoryCache.Default;
        List<ProductCategory> productCategories;

        public ProductCategoryRepository()
        {
            productCategories = cache["productCategories"] as List<ProductCategory>;
            if (productCategories == null)
            {
                productCategories = new List<ProductCategory>();
            }
        }

        public void Commit()
        {
            cache["productCategories"] = productCategories; 
        }

        public void Insert(ProductCategory productCategory)
        {
            productCategories.Add(productCategory);
        }

        public void Update(ProductCategory productCategory)
        {
            ProductCategory productCategoryToUpdate = productCategories.Find(pc => pc.Id == productCategory.Id);
            if (productCategoryToUpdate == null)
            {
                throw new Exception("Product Category not found");
            }
            else
            {
                productCategoryToUpdate = productCategory;
            }
        }

        public ProductCategory Find(string id)
        {
            ProductCategory productCategory = productCategories.Find(pc => pc.Id == id);
            if (productCategory == null)
            {
                throw new Exception("Product Category not found");
            }
            else
            {
                return productCategory;
            }
        }

        public IQueryable<ProductCategory> Collection()
        {
            return productCategories.AsQueryable();
        }

        public void Delete(string id)
        {
            ProductCategory productCategoryToDelete = productCategories.Find(pc => pc.Id == id);
            if (productCategoryToDelete == null)
            {
                throw new Exception("Product Category not found");
            }
            else
            {
                productCategories.Remove(productCategoryToDelete);
            }
        }
    }
}
