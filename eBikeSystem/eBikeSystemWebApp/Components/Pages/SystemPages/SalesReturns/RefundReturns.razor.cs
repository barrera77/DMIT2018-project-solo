using BlazorDialog;
using eBikeSystemDB.BLL.SalesReturns;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using eBikeSystemDB.ViewModels;


namespace eBikeSystemWebApp.Components.Pages.SystemPages.SalesReturns
{
    public partial class RefundReturns : ComponentBase
    {

        #region Properties      

        [Inject]
        protected IDialogService DialogService { get; set; }

        [Inject]
        protected IBlazorDialogService BlazorDialogService { get; set; }

        [Inject]
        private NavigationManager _navManager { get; set; }

        [Inject]
        ReturnServices ReturnServices { get; set; }

        #endregion

        #region Fields
        public List<SaleRefundDetailView> saleRefundDetailList { get; set; } = new List<SaleRefundDetailView>();    
        public SaleRefundView? SaleRefund { get; set; } = new SaleRefundView();

        [Parameter]
        public int SaleID { get; set; }

        [Parameter]
        public int Qty { get; set; } = 0;

        private decimal SaleTotal { get; set; } = 0;
        private decimal Discount {  get; set; } = 0;

        #endregion

        #region Feedback and Error Messaging

        private string feedbackMessage;
        private string errorMessage;

        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);

        private List<string> errorDetails = new List<string>();

        #endregion


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            try
            {
                
            }
            catch (AggregateException ex)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage += Environment.NewLine;
                }

                foreach (Exception error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;

            }
            catch (Exception ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }

            await InvokeAsync(StateHasChanged);
        }


        public void Search(int saleId)
        {
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;
            saleRefundDetailList.Clear();

            try
            {
               
                SaleRefund = ReturnServices.SaleRefund_GetSaleRefund(saleId);
                saleRefundDetailList = ReturnServices.SaleRefund_GetSaleDetailsRefund(saleId);

                SaleRefund.SubTotal = Math.Round((SaleRefund.SubTotal - SaleRefund.TaxAmount), 2);
                SaleRefund.TaxAmount = Math.Round(SaleRefund.TaxAmount, 2);
                

                SaleTotal = Math.Round((SaleRefund.SubTotal + SaleRefund.TaxAmount), 2);
                Discount = Math.Round(((SaleRefund.SubTotal * SaleRefund.DiscountPercent) / 100), 2);
            }
            catch (AggregateException ex)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage += Environment.NewLine;
                }

                foreach (Exception error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;

            }
            catch (Exception ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
            
        }


        public void Clear()
        {
            SaleRefund = new SaleRefundView();
            saleRefundDetailList.Clear();
            SaleTotal = 0;
            Discount = 0;
            SaleID = 0;
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;
        }

        private void OnProcessRefund()
        {
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;

            try
            {
                SaleRefund = ReturnServices.SaleRefund_Refund( SaleRefund, saleRefundDetailList);
            }
            catch (AggregateException ex)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage += Environment.NewLine;
                }

                foreach (Exception error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;

            }
            catch (Exception ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }

        }

        
    }
}
