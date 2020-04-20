using System;
using System.ComponentModel.DataAnnotations;
using nhitomi.Models.Validation;

namespace nhitomi.Models
{
    public class User : UserBase, INanokaObject, IHasUpdatedTime
    {
        /// <summary>
        /// User ID.
        /// </summary>
        [Required, NanokaId]
        public string Id { get; set; }

        /// <summary>
        /// Time when this user was created.
        /// </summary>
        [Required]
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// Time when this user was updated.
        /// </summary>
        [Required]
        public DateTime UpdatedTime { get; set; }

        /// <summary>
        /// User username.
        /// </summary>
        [Required, MinLength(UsernameMinLength), MaxLength(UsernameMaxLength), RegularExpression(UsernameRegex)]
        public string Username { get; set; }

        /// <summary>
        /// User email.
        /// </summary>
        /// <remarks>
        /// This value is only specified when the requesting user has view permissions.
        /// </remarks>
        public string Email { get; set; }

        /// <summary>
        /// List of all restrictions on this user.
        /// </summary>
        [Required]
        public UserRestriction[] Restrictions { get; set; }

        /// <summary>
        /// User permissions.
        /// </summary>
        [Required]
        public UserPermissions[] Permissions { get; set; }
    }

    public class UserBase
    {
        public const string UsernameRegex = @"^(?=.{4,20}$)(?![_.])(?!.*[_.]{2})[a-zA-Z0-9._]+(?<![_.])$";

        public const int UsernameMinLength = 4;
        public const int UsernameMaxLength = 20;

        public const int PasswordMinLength = 6;
        public const int PasswordMaxLength = 2048;
    }
}