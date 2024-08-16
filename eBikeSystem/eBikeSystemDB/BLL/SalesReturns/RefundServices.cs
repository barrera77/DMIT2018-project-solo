using eBikeSystemDB.DAL;
using eBikeSystemDB.Entities;
using eBikeSystemDB.ViewModels;


namespace eBikeSystemDB.BLL.SalesReturns
{
    public class RefundServices
    {
        private readonly eBikeContext _context;

        internal RefundServices(eBikeContext context)
        {
            _context = context;
        }


        private string errorMessage;

        //Create an array to store the errors
        List<Exception> errorList = new List<Exception>();


        public SaleRefundView SaleRefund_GetSaleRefund(int saleId)
        {
            return _context.SaleRefunds
                    .Where(s => s.SaleId == saleId)
                    .Select(s => new SaleRefundView
                    {
                        SaleID = s.SaleId,
                        EmployeeID = s.EmployeeId,
                        TaxAmount = s.TaxAmount,
                        SubTotal = s.SubTotal,
                        DiscountPercent = (int)(s.SubTotal * 0.15m)
                    }).FirstOrDefault();

        }

        public SaleRefundView SaleRefund_Refund(SaleRefundView saleRefund, List<SaleRefundDetailView> saleRefundDetails)
        {

            foreach (var saleRefundDetail in saleRefundDetails)
            {
                if (saleRefundDetail.Quantity < 0)
                {
                    errorList.Add(new Exception($"Quantity for item {saleRefundDetail.Description} is less than zero (0)"));
                }

                if (saleRefundDetail.Quantity > 0 && String.IsNullOrWhiteSpace(saleRefundDetail.Reason))
                {
                    errorList.Add(new Exception($"There is no reason given for item {saleRefundDetail.Description}"));
                }

                if (saleRefundDetail.Quantity > (saleRefundDetail.OriginalQuantity - saleRefundDetail.ReturnQuantity))
                {
                    errorList.Add(new Exception($"Quantity for item {saleRefundDetail.Description} is greater than {saleRefundDetail.OriginalQuantity - saleRefundDetail.ReturnQuantity}"));
                }

                if (!saleRefundDetail.Refundable)
                {
                    errorList.Add(new Exception($"Item {saleRefundDetail.Description} is not refundable"));
                }

                if (errorList.Count > 0)
                {
                    throw new AggregateException("Unable to Save the sale. Check errors:", errorList);
                }

                SaleRefund newSaleRefund = new SaleRefund();
                newSaleRefund.SaleRefundDate = DateTime.Now;
                newSaleRefund.SaleId = saleRefund.SaleID;
                newSaleRefund.SubTotal = saleRefund.SubTotal;
                newSaleRefund.TaxAmount = saleRefund.TaxAmount;
                


                //TODO: Finish refund method
            }

            return saleRefund;
        }


    }
       
}
