using eBikeSystemDB.DAL;
using eBikeSystemDB.Entities;
using eBikeSystemDB.ViewModels;


namespace eBikeSystemDB.BLL.SalesReturns
{
    public class ReturnServices
    {

        private readonly eBikeContext _context;

        internal ReturnServices(eBikeContext context)
        {
            _context = context;
        }

        private string errorMessage;

        //Create an array to store the errors
        List<Exception> errorList = new List<Exception>();

        #region Return Query Methods

        public SaleRefundView? SaleRefund_GetSaleRefund(int saleId)
        {
            bool saleExists = _context.Sales.Any(s => s.SaleId == saleId);
            if(!saleExists)
            {
                throw new ArgumentNullException("No sales ID found");

            }
            return _context.Sales
                    .Where(s => s.SaleId == saleId)
                    .Select(s => new SaleRefundView
                    {
                        SaleID = s.SaleId,
                        EmployeeID = s.EmployeeId,
                        TaxAmount = s.TaxAmount,
                        SubTotal = s.SubTotal,
                        DiscountPercent = s.CouponId == null ? 0 : s.Coupon.CouponDiscount,
                    })
                    .FirstOrDefault();
        }

        public List<SaleRefundDetailView> SaleRefund_GetSaleDetailsRefund(int saleId)
        {
            bool saleExists = _context.Sales.Any(s => s.SaleId == saleId);
            if (!saleExists)
            {
                throw new ArgumentNullException("No sales ID found");

            }

            return _context.SaleDetails
                    .Where(sd => sd.SaleId == saleId)
                    .Select(sd => new SaleRefundDetailView
                    {
                        PartID = sd.PartId,
                        Description = sd.Part.Description,
                        OriginalQuantity = sd.Quantity,
                        SellingPrice = sd.SellingPrice,
                        ReturnQuantity = _context.SaleRefundDetails
                                        .Where(srf => srf.SaleRefund.SaleId == sd.SaleId)
                                        .Select(srf => srf.Quantity).Sum(),
                        Refundable = sd.Part.Refundable == "Y",
                        Quantity = 0,
                        Reason = string.Empty,
                    })
                    .ToList();
        }

        #endregion


        public void SaleFefund_GetRefund(int saleId)
        {
            bool saleExists = _context.Sales.Any(s => s.SaleId == saleId);

            if (!saleExists)
            {
                throw new ArgumentNullException("No sales ID found");
            }
            else
            {
                SaleRefund_GetSaleRefund(saleId);
                SaleRefund_GetSaleDetailsRefund(saleId);
            }
        }
    }
}
