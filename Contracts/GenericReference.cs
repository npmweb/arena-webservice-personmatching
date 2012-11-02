using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Arena.Custom.NPM.WebServiceMatching.Contracts
{
    [DataContract(Namespace = "")]
    public class GenericReference
    {
        [DataMember(EmitDefaultValue = false)]
        public int ID { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Title { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Value { get; set; }

        public GenericReference(GenericReferenceType arena)
        {
            ID = arena.ID;
            Title = arena.Title;
            Value = arena.Value;
        }

        public GenericReference(Core.Person arena)
        {
            ID = arena.PersonID;
            Title = arena.FullName;
        }

        public GenericReference(Arena.SmallGroup.Category arena)
        {
            ID = arena.CategoryID;
            Title = arena.CategoryName;
            Value = arena.NameCaption;
        }

        public GenericReference(Arena.SmallGroup.GroupCluster arena)
        {
            ID = arena.GroupClusterID;
            Title = arena.Name;
        }

        public GenericReference(Arena.SmallGroup.Group arena)
        {
            ID = arena.GroupID;
            Title = arena.Name;
        }

        public GenericReference(Core.Profile arena)
        {
            ID = arena.ProfileID;
            Title = arena.Name;
        }

        public GenericReference() { }

        public GenericReference(int id, string title, string value)
        {
            ID = id;
            Title = title;
            Value = value;
        }
    }

    [DataContract(Namespace = "")]
    public class GenericReferenceType
    {
        [DataMember(EmitDefaultValue = true)]
        public int ID { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public string Title { get; set; }

        //NPM: Add a value
        [DataMember(EmitDefaultValue = true)]
        public string Value { get; set; }
    }    

    //[DataContract(Namespace = "")]
    public class EmailReference
    {
        [DataMember(EmitDefaultValue = false)]
        public int Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Address { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string IsActive { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Order { get; set; }

        public EmailReference() { }

        public EmailReference(int id, string address, string isActive, string order)
        {
            Id = id;
            Address = address;
            IsActive = isActive;
            Order = order.ToString();
        }
    }    

    [DataContract(Namespace = "")]
    public class PersonReference
    {
        [DataMember(EmitDefaultValue = false)]
        public string ActivationAttempts { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Age { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string AnniversaryDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string BirthDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Campus { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string City { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Contracts.GenericListResult<Contracts.EmailReference> Emails { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int FamilyId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string FamilyIsChild { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string FirstName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Gender { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string HasActiveKey { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string HasLogin { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string LastName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string MaritalStatus { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string MatchPercent { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string NickName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int PersonId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Contracts.GenericListResult<Contracts.PhoneReference> PhoneNumbers { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string PostalCode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool RecordStatusActive { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SpouseFirstName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SpouseNickName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string State { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Street1 { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Street2 { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string IsExact { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string IsExactPrimaryEmailDomain { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string MatchMap { get; set; }
    }

    //[DataContract(Namespace = "")]
    public class PhoneReference
    {
        [DataMember(EmitDefaultValue = false)]
        public string Number { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Extension { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Type { get; set; }

        public PhoneReference() { }

        public PhoneReference(string number, string extension, string type)
        {
            Number = number;
            Extension = extension;
            Type = type;
        }
    }
}