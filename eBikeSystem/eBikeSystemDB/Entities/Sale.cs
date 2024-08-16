﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eBikeSystemDB.Entities;

public partial class Sale
{
    [Key]
    [Column("SaleID")]
    public int SaleId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SaleDate { get; set; }

    [Column("EmployeeID")]
    public int EmployeeId { get; set; }

    [Column(TypeName = "money")]
    public decimal TaxAmount { get; set; }

    [Column(TypeName = "money")]
    public decimal SubTotal { get; set; }

    [Column("CouponID")]
    public int? CouponId { get; set; }

    [Required]
    [StringLength(1)]
    public string PaymentType { get; set; }

    [ForeignKey("CouponId")]
    [InverseProperty("Sales")]
    public virtual Coupon Coupon { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("Sales")]
    public virtual Employee Employee { get; set; }

    [InverseProperty("Sale")]
    public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();

    [InverseProperty("Sale")]
    public virtual ICollection<SaleRefund> SaleRefunds { get; set; } = new List<SaleRefund>();
}