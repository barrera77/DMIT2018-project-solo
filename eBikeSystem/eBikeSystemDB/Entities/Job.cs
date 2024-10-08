﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eBikeSystemDB.Entities;

public partial class Job
{
    [Key]
    [Column("JobID")]
    public int JobId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime JobDateIn { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? JobDateStarted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? JobDateDone { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? JobDateOut { get; set; }

    [Column("EmployeeID")]
    public int EmployeeId { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal ShopRate { get; set; }

    [Required]
    [StringLength(13)]
    public string VehicleIdentification { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("Jobs")]
    public virtual Employee Employee { get; set; }

    [InverseProperty("Job")]
    public virtual ICollection<JobDetail> JobDetails { get; set; } = new List<JobDetail>();

    [ForeignKey("VehicleIdentification")]
    [InverseProperty("Jobs")]
    public virtual CustomerVehicle VehicleIdentificationNavigation { get; set; }
}