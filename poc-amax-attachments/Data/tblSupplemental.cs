using System.ComponentModel.DataAnnotations;

namespace poc_amax_attachments.Data
{
    public class tblSupplemental
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OwnerId { get; set; }

        public int OwnerType { get; set; }

        public int ObjectType { get; set; }

        [StringLength(255)]
        public string ObjectName { get; set; }

        [StringLength(255)]
        public string FileName { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public short EmailFolder { get; set; }

        [StringLength(100)]
        public string EmailUidl { get; set; }

        public byte[]? PrivateKey { get; set; }

        [StringLength(39)]
        public string Md5 { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        [Required]
        public DateTime DateModified { get; set; }

        public int DeletedBy { get; set; }

        [Required]
        public bool Deleted { get; set; }

        [StringLength(255)]
        public string NewFilename { get; set; }

        [StringLength(255)]
        public string OldFilename { get; set; }

        public bool FinishedRename { get; set; }

        public DateTime SysUtcDateCreated { get; set; }

        public DateTime SysUtcDateModified { get; set; }

        public string Workstation { get; set; }

        public int? AgentId { get; set; }

        [StringLength(20)]
        public string IpAddress { get; set; }

        public bool? UploadCompleted { get; set; }
    }
}
