using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace MyCompany.MyProduct.Samples
{
    public class Sample : FullAuditedAggregateRoot<Guid>
    {
        private string _description;
        private string _name;

        public Sample(
            Guid id,
            string name)
        {
            Id = id;
            Name = name;
        }

        private Sample()
        {
        }

        public string Description
        {
            get => _description;
            set
            {
                Check.Length(value, nameof(Description), SampleConsts.DescriptionMaxLength, 0);
                _description = value;
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                Check.NotNull(value, nameof(Name));
                Check.Length(value, nameof(Name), SampleConsts.NameMaxLength, 0);
                _name = value;
            }
        }
    }
}