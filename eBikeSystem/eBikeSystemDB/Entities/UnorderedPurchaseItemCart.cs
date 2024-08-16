﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eBikeSystemDB.Entities;

[Table("UnorderedPurchaseItemCart")]
public partial class UnorderedPurchaseItemCart
{
    [Key]
    [Column("CartID")]
    public int CartId { get; set; }

    [Required]
    [StringLength(100)]
    public string Description { get; set; }

    [Required]
    [StringLength(50)]
    public string VendorPartNumber { get; set; }

    public int Quantity { get; set; }
}