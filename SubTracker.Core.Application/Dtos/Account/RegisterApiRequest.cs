using System.ComponentModel.DataAnnotations;

namespace SubTracker.Core.Application.Dtos.Account
{
    public class RegisterApiRequest
    {
        [Required(ErrorMessage = "El nombre es requerido.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "El correo electrónico debe tener un formato válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es requerido.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de contraseña es requerida.")]
        [Compare(nameof(Password), ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
