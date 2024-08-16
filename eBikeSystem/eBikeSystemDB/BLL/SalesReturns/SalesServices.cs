using eBikeSystemDB.DAL;
using eBikeSystemDB.Entities;
using eBikeSystemDB.ViewModels;

namespace eBikeSystemDB.BLL.SalesReturns
{
    public class SalesServices
    {
        private readonly eBikeContext _context;

        internal SalesServices(eBikeContext context)
        {
            _context = context;
        }


        private string errorMessage;

        //Create an array to store the errors
        List<Exception> errorList = new List<Exception>();


        #region Query Service Methods
        /// <summary>
        /// Gets the part categories
        /// </summary>
        /// <returns>A list of part categories</returns>
        public List<CategoryView> Sale_GetCategories()
        {
            return _context.Categories
                    .Select(c => new CategoryView
                    {
                        CategoryID = c.CategoryId,
                        Description = c.Description
                    }).ToList();
        }

        /// <summary>
        /// Gets the parts
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns>A list of all available parts excluding those that have been discontinued</returns>
        public List<PartView> Sale_GetParts(int categoryId)
        {
            if (categoryId == 0)
            {
                throw new ArgumentNullException("Please provide a valid category");
            }
            return _context.Parts
                    .Where(p => p.CategoryId == categoryId && !p.Discontinued)
                    .Select(p => new PartView
                    {
                        PartID = p.PartId,
                        Description = p.Description,
                        SellingPrice = p.SellingPrice
                    }).ToList();
        }

        /// <summary>
        /// Gets the coupon amount
        /// </summary>
        /// <param name="coupons"></param>
        /// <returns>The discount applicable to the provided coupon</returns>
        public int GetCoupon(string coupons)
        {
            if (String.IsNullOrWhiteSpace(coupons))
            {
                throw new ArgumentException("The provided coupon does not exist");
            }

            return _context.Coupons
                .Where(c => c.CouponIdvalue == coupons)
                .Select(c => String.IsNullOrWhiteSpace(coupons) ? 0 : c.CouponDiscount).FirstOrDefault();
        }

        #endregion

        /// <summary>
        /// Checkout and save the same and SaleDetails in the DB
        /// </summary>
        /// <param name="sale"></param>
        /// <param name="saleDetails"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="AggregateException"></exception>

        public int Checkout(SaleView sale, List<SaleDetailView> saleDetails)
        {

            #region Business Logic
            List<Exception> errorList = new List<Exception>();

            if (sale == null)
            {
                throw new ArgumentException("There are no items in your cart");

            }

            foreach (var saleDetail in saleDetails)
            {
                if (saleDetail.Quantity <= 0)
                {
                    errorList.Add(new Exception("Quantity cannot be 0 or less than 0"));
                }

            }

            if (errorList.Count > 0)
            {
                throw new AggregateException("Unable to Save the sale.  Check errors:", errorList);
            }

            #endregion

            //Create new Sale
            Sale newSale = new Sale
            {
                SaleDate = DateTime.Now,
                EmployeeId = 9,
                TaxAmount = sale.TaxAmount,
                SubTotal = sale.SubTotal,
                PaymentType = sale.PaymentType,
                CouponId = sale.CouponId <= 0 ? null : sale.CouponId,
            };




            foreach (var saleDetail in saleDetails)
            {
                if (saleDetail == null)
                {
                    errorList.Add(new Exception("Sale detail not found"));
                }
                else
                {
                    //Create the new SaleDetails
                    SaleDetail newSaleDetail = new SaleDetail
                    {
                        SaleId = newSale.SaleId,
                        PartId = saleDetail.PartID,
                        Quantity = saleDetail.Quantity,
                        SellingPrice = saleDetail.SellingPrice,

                    };

                    // Check for the part
                    Part part = _context.Parts.FirstOrDefault(p => p.PartId == saleDetail.PartID);

                    if (part.QuantityOnHand < saleDetail.Quantity)
                    {
                        errorList.Add(new Exception($"Insufficient quantity for part {saleDetail.PartID}. Available: {part.QuantityOnHand}, Requested: {saleDetail.Quantity}"));
                    }

                    // Deduct the amount sold from the inventory
                    part.QuantityOnHand -= saleDetail.Quantity;
                    _context.Parts.Update(part);

                    newSale.SaleDetails.Add(newSaleDetail);
                }
            }
            if (errorList.Count > 0)
            {
                throw new AggregateException("Unable to Save the sale.  Check errors:", errorList);
            }
            else
            {
                if (newSale.SaleId <= 0)
                {
                    _context.Sales.Add(newSale);
                }
                _context.SaveChanges();
            }
            return newSale.SaleId;
        }

        public int GetCouponId(string coupon)
        {
            if (String.IsNullOrWhiteSpace(coupon))
            {
                return 0;
            }

            return _context.Coupons
                            .Where(c => c.CouponIdvalue == coupon)
                            .Select(c => c.CouponId).FirstOrDefault();
        }
    }
}
