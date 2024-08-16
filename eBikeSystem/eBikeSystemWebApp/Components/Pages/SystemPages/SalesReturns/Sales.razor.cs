using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using eBikeSystemDB.BLL.SalesReturns;
using eBikeSystemDB.ViewModels;
using MudBlazor;
using BlazorDialog;
using Microsoft.AspNetCore.Components.QuickGrid;
using eBikeSystemDB.Entities;

namespace eBikeSystemWebApp.Components.Pages.SystemPages.SalesReturns
{
    public partial class Sales : ComponentBase
    {

        #region Properties      

        [Inject]
        protected IDialogService DialogService { get; set; }

        [Inject]
        protected IBlazorDialogService BlazorDialogService { get; set; }

        [Inject]
        private NavigationManager _navManager { get; set; }

        [Inject]
        SalesServices SalesServices { get; set; }

        [Parameter]
        public int CategoryId { get; set; }

        [Parameter]
        public int PartId { get; set; }

        [Parameter]
        public string Description { get; set; }

        [Parameter]
        public int Quantity { get; set; }

        [Parameter]
        public decimal SellingPrice { get; set; }

        [Parameter]
        public decimal Total { get; set; }

        [Parameter]
        public int SaleId { get; set; }

        #endregion


        #region Feedback and Error Messaging

        private string feedbackMessage;
        private string errorMessage;

        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);

        private List<string> errorDetails = new List<string>();       

        #endregion

       

        #region Fields
        public List<CategoryView> CategoryList { get; set; } = new List<CategoryView>();
        public List<PartView> PartsList { get; set; } = new List<PartView>();

        public SaleDetailView NewSaleDetail { get; set; }
        public List<SaleDetailView> SaleDetailList { get; set; } = new List<SaleDetailView>();
        public SaleView NewSale { get; set; } = new SaleView();

        public decimal SaleSubTotal = 0;

        private List<string> PaymentTypeList { get; set; }

        private Coupon Coupon { get; set; }

        private string CouponName { get; set; }
        private int CouponValue { get; set; }
        private decimal Discount { get; set; }

        #endregion

        #region Validation

        private EditContext editContext;
        private ValidationMessageStore messageStore;
        private bool isSaleCompleted;

        #endregion


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            PaymentTypeList = new List<string>
            {
                "Cash",
                "Credit",
                "Debit"
            };

            try
            {
                //get the categories
                CategoryList = SalesServices.Sale_GetCategories();

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

        /// <summary>
        /// Get the list of parts upon selecting an option from the dropdown list
        /// </summary>
        /// <param name="e"></param>
        /// <returns>The list of parts corresponding to the selected category</returns>
        private async Task OnHandleSelectedCategory(ChangeEventArgs e)
        {
            CategoryId = Convert.ToInt32(e.Value);

            if (CategoryId >= 0)
            {
                PartsList = SalesServices.Sale_GetParts(CategoryId);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// Creates a newSaleDetail to display in the table
        /// </summary>
        public void CreateNewSaleDetail()
        {
            errorDetails.Clear();
            errorMessage = string.Empty;

            if (PartId > 0 && Quantity > 0)
            {
                try
                {
                    AddPart(PartId);
                    UpdateSaleTotals();

                    //reset fields
                    CategoryId = 0;
                    Quantity = 0;
                    PartId = 0;
                    PartsList.Clear();

                    if (Discount > 0)
                    {
                        if (Discount > 0)
                        {
                            Discount = Math.Round((SaleSubTotal * NewSale.DiscountPercent) / 100M, 2);
                        }
                    }

                    StateHasChanged();

                }
                catch (AggregateException ex)
                {
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        errorMessage += Environment.NewLine;
                    }
                    errorMessage += "Item already exists";
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
            else
            {
                errorMessage = "Please select a product and enter a valid quantity.";
            }
        }

        /// <summary>
        /// Adds a sale detail basedon the selected part/item
        /// </summary>
        /// <param name="partId"></param>
        public void AddPart(int partId)
        {
            errorDetails.Clear();
            errorMessage = string.Empty;

            // Check for duplicates
            //Option 1
            //bool isItemDuplicate = SaleDetailList.Any(sd => sd.PartID == PartId);
            //if (isItemDuplicate)
            //{
            //    errorMessage = "Duplicate items in the cart are not allowed.";               

            //}

            //Option 2
            SaleDetailView duplicatedPart = SaleDetailList.FirstOrDefault(dp => dp.PartID == partId);

            if (duplicatedPart != null)
            {
                //update quanty of existing item in cart
                duplicatedPart.Quantity += Quantity;
                duplicatedPart.Total = duplicatedPart.SellingPrice * duplicatedPart.Quantity;
                NewSale = new SaleView();
            }
            else
            {
                PartView selectedPart = PartsList.FirstOrDefault(p => p.PartID == PartId);

                NewSaleDetail = new SaleDetailView
                {
                    PartID = PartId,
                    Description = selectedPart.Description,
                    Quantity = Quantity,
                    SellingPrice = Math.Round((selectedPart.SellingPrice), 2),
                    Total = Math.Round((selectedPart.SellingPrice * Quantity), 2),
                };

                SaleDetailList.Add(NewSaleDetail);
                UpdateSaleTotals();
            }

        }

        /// <summary>
        /// Removes an item from the cart and updates quantity and totals
        /// </summary>
        /// <param name="partId"></param>
        public void RemovePart(int partId)
        {
            if (partId > 0)
            {
                SaleDetailView partToRemove = SaleDetailList.FirstOrDefault(p => p.PartID == partId);

                if (partToRemove != null)
                {
                    SaleDetailList.Remove(partToRemove);

                    //update quantity and total of the cart
                    //Quantity -= partToRemove.Quantity;
                    Total -= Math.Round((partToRemove.Total), 2);

                }
            }
        }

        /// <summary>
        /// Updates part/item totals upon changing quantity
        /// </summary>
        /// <param name="partId"></param>
        public void RefreshPart(int partId)
        {
            if (partId > 0)
            {
                SaleDetailView partToUpdate = SaleDetailList.FirstOrDefault(p => p.PartID == partId);

                if (partToUpdate != null)
                {
                    if (partToUpdate.Quantity == 0)
                    {
                        try
                        {
                            RemovePart(partId);
                        }
                        catch (AggregateException ex)
                        {
                            if (!string.IsNullOrWhiteSpace(errorMessage))
                            {
                                errorMessage += Environment.NewLine;
                            }
                            errorMessage += "Item already exists";
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
                    else
                    {
                        partToUpdate.Total = Math.Round((partToUpdate.SellingPrice * partToUpdate.Quantity), 2);
                        UpdateSaleTotals();
                    }
                    StateHasChanged();
                }
            }
        }

        public void Clear()
        {
            //reset fields
            CategoryId = 0;
            Quantity = 0;
            PartId = 0;
            PartsList.Clear();
            CouponName = "";
            NewSale.DiscountPercent = 0;
            SaleSubTotal = 0;
            NewSale.TaxAmount = 0;
            Discount = 0;
            NewSale.SubTotal = 0;
            SaleDetailList.Clear();
        }

        private void UpdateSaleTotals()
        {
            //get the sale subtotal by adding the cost of all products/items
            decimal subtotal = SaleDetailList.Sum(detail => detail.Total);

            SaleSubTotal = Math.Round(subtotal, 2);
            NewSale.TaxAmount = Math.Round((subtotal * 0.05m), 2); // 5% tax

            NewSale.SubTotal = Math.Round(((SaleSubTotal + NewSale.TaxAmount) - Discount), 2);

        }

        public void OnCheckout()
        {
            try
            {
                errorDetails.Clear();
                errorMessage = string.Empty;
                feedbackMessage = string.Empty;

                NewSale.CouponId = SalesServices.GetCouponId(CouponName);
                        
                //Format the payment type
                if (NewSale.PaymentType == "Debit")
                {
                    NewSale.PaymentType = "D";
                }
                if (NewSale.PaymentType == "Credit")
                {
                    NewSale.PaymentType = "C";
                }
                if (NewSale.PaymentType == "Cash")
                {
                    NewSale.PaymentType = "M";
                }

                SaleId = SalesServices.Checkout(NewSale, SaleDetailList);

                feedbackMessage = "Thank you for your purchase";
                isSaleCompleted = true;
                Clear();

            }
            catch (AggregateException ex)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage += Environment.NewLine;
                }
                errorMessage += "Item already exists";
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

            StateHasChanged();
        }



        public void OnVerifyCoupon(string coupon)
        {
            if (String.IsNullOrWhiteSpace(coupon))
            {
                errorMessage = "Coupon cannot be null";
            }
            else
            {
                try
                {
                    //Processor the coupon
                    VerifyCoupon(coupon);

                    //Re-calculate the total sale amount
                    NewSale.SubTotal = Math.Round((SaleSubTotal - Discount), 2);

                }
                catch (AggregateException ex)
                {
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        errorMessage += Environment.NewLine;
                    }
                    errorMessage += "Invalid coupon";
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

        public decimal VerifyCoupon(string coupon)
        {
            NewSale.DiscountPercent = SalesServices.GetCoupon(coupon);

            decimal convertedDiscount = Convert.ToDecimal(NewSale.DiscountPercent);

            return Discount = Math.Round((SaleSubTotal * convertedDiscount) / 100M, 2);
        }

        private void OnProductSelected(int partId)
        {
            // Set the selected PartId
            PartId = partId;


            // Set the default quantity to 1 if it is currently 0
            if (Quantity == 0)
            {
                Quantity = 1;
            }

            // Ensure UI updates to reflect this change
            StateHasChanged();
        }

        private async Task OnEmptyCart()
        {
            if(NewSaleDetail == null)
            {
                errorMessage = "There are no items in your cart";
            }
            else
            {
                string bodyText = $"Are you sure you want to empty your cart?";

                string dialogResult = await BlazorDialogService.ShowComponentAsDialog<string>(
                    new ComponentAsDialogOptions(typeof(SimpleComponentDialog))
                    {
                        Size = DialogSize.Normal,
                        Parameters = new()
                            {
                            { nameof(SimpleComponentDialog.Input), "Shopping Cart" },
                            { nameof(SimpleComponentDialog.BodyText), bodyText }
                            }
                    });

                if (dialogResult == "Ok")
                {
                    Clear();
                }
            }
            await InvokeAsync(StateHasChanged);
        }
    }
}
