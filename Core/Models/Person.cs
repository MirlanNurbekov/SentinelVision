using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    /// <summary>
    /// Represents an individual in the face‐recognition system.
    /// </summary>
    [Table("Persons")]
    public class Person : IEquatable<Person>
    {
        /// <summary>
        /// Primary key identifier.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        /// <summary>
        /// Full name of the person.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FullName { get; private set; }

        /// <summary>
        /// Binary‐encoded face template (e.g., 128‐dim embedding).
        /// </summary>
        [Required]
        public byte[] FaceTemplate { get; private set; }

        /// <summary>
        /// Record creation timestamp.
        /// </summary>
        public DateTimeOffset CreatedAt { get; private set; }

        /// <summary>
        /// Last update timestamp.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; private set; }

        /// <summary>
        /// EF Core constructor.
        /// </summary>
        private Person() { }

        /// <summary>
        /// Creates a new person with the specified name and face template.
        /// </summary>
        /// <param name="fullName">Non‐empty full name.</param>
        /// <param name="faceTemplate">Non‐empty face template bytes.</param>
        public Person(string fullName, byte[] faceTemplate)
        {
            UpdateName(fullName);
            UpdateTemplate(faceTemplate);
            CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Updates the person's name.
        /// </summary>
        public void UpdateName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("FullName must be provided.", nameof(fullName));

            FullName = fullName.Trim();
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Replaces the face template.
        /// </summary>
        public void UpdateTemplate(byte[] template)
        {
            if (template == null || template.Length == 0)
                throw new ArgumentException("FaceTemplate must be non‐empty.", nameof(template));

            FaceTemplate = (byte[])template.Clone();
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public override bool Equals(object obj) => Equals(obj as Person);

        public bool Equals(Person other) =>
            other != null &&
            Id == other.Id &&
            FullName == other.FullName;

        public override int GetHashCode() =>
            HashCode.Combine(Id, FullName);

        public override string ToString() =>
            $"Person {{ Id = {Id}, Name = {FullName} }}";
    }
}
