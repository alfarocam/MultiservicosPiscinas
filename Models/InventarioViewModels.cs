using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MultiserviciosPiscinas.Models;

public class InventarioIndexViewModel
{
    public List<InventarioItemViewModel> Productos { get; set; } = new();
}

public class InventarioItemViewModel
{
    public int Id { get; set; }

    public string Categoria { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public bool Activo { get; set; }
}

public class InventarioFormularioViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una categoría.")]
    [Display(Name = "Categoría")]
    public int? CategoriaId { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    [Display(Name = "Nombre del producto")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "El precio debe ser mayor a 0.")]
    [Display(Name = "Precio")]
    public decimal? Precio { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(0, 999999, ErrorMessage = "La cantidad no puede ser negativa.")]
    [Display(Name = "Cantidad")]
    public int? Stock { get; set; }

    [StringLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;

    public List<SelectListItem> Categorias { get; set; } = new();
}