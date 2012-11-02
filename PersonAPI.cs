using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Web;
using Arena.Core;
using Arena.Security;
using Arena.SmallGroup;

using Arena.Custom.NPM.DataLayer.Data;
using Arena.Custom.NPM.DataLayer.Services;

namespace Arena.Custom.NPM.WebServiceMatching
{
    /// <summary>
    /// PersonAPI handles all methods and functions dealing with the Arena Person and related objects.
    /// </summary>
    public class PersonAPI
    {
        #region NPM
        /// <summary>
        /// Based on the parameters all potentially matching Persons are returned.<para></para>
        /// <b>WEBSERVICE</b><para></para>
        /// Action - GET<para></para>
        /// Method - person/get/match/gender/{gender}/maritalstatus/{maritalstatus}/?email={email}&firstname={firstname}&lastname={lastname}&phone={phone}&birthdate={birthdate}&street1={street1}&street2={street2}&city={city}&state={state}&postalcode={postalcode}&campus={campus}&matchpercentminimum={matchpercentminimum}&sessionid={sessionid}<para></para>
        /// Security - Builtin<para></para>
        /// PLEASE NOTE: MatchMap Positions from Left to Right
        /// Position 1 = Email
        /// Position 2 = FirstName
        /// Position 3 = LastName
        /// Position 4 = Gender
        /// Position 5 = MaritalStatus
        /// Position 6 = Campus
        /// Position 7 = Phone (Main/Home)
        /// Position 8 = Phone (Cell)
        /// Position 9 = Birthday
        /// Position 10 = Street1
        /// Position 11 = Street2
        /// Position 12 = City
        /// Position 13 = State
        /// Position 14 = PostalCode
        /// Position 15 = RecordStatus
        /// </summary>
        /// <param name="email"></param>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        /// <param name="gender"></param>
        /// <param name="maritalstatus"></param>
        /// <param name="campus"></param>
        /// <param name="street1"></param>
        /// <param name="street2"></param>
        /// <param name="city"></param>
        /// <param name="state"></param>
        /// <param name="postalcode"></param>
        /// <param name="phone"></param>
        /// <param name="birthdate"></param>
        /// <param name="matchpercentminimum"></param>
        /// <param name="sessionid"></param>
        /// <returns>GenericListResult of PersonReference</returns>
        [WebGet(UriTemplate = "person/get/match/gender/{gender}/maritalstatus/{maritalstatus}/?email={email}&firstname={firstname}&lastname={lastname}&phonehome={phonehome}&phonecell={phonecell}&birthdate={birthdate}&street1={street1}&street2={street2}&city={city}&state={state}&postalcode={postalcode}&campus={campus}&matchpercentminimum={matchpercentminimum}&sessionid={sessionid}")]
        public Contracts.GenericListResult<Contracts.PersonReference> GetPersonMatch(string email, string firstname, string lastname, string gender, string maritalstatus, string campus, string street1, string street2, string city, string state, string postalcode, string phonehome, string phonecell, string birthdate, string matchpercentminimum, string sessionid)
        {
            Contracts.GenericListResult<Contracts.PersonReference> personReferenceList = new Contracts.GenericListResult<Contracts.PersonReference>();
            personReferenceList.Items = new List<Contracts.PersonReference>();

            // Load PersonReference object with the provided values
            Contracts.PersonReference personReferenceOriginal = new Contracts.PersonReference();
            personReferenceOriginal.BirthDate = birthdate.Trim();
            personReferenceOriginal.Campus = campus.Trim();
            personReferenceOriginal.City = city.Trim();
            personReferenceOriginal.Emails = new Contracts.GenericListResult<Contracts.EmailReference>();
            personReferenceOriginal.Emails.Items = new List<Contracts.EmailReference>();
            personReferenceOriginal.Emails.Items.Add(new Contracts.EmailReference(1, email.Trim(), "True", "1"));
            personReferenceOriginal.FirstName = firstname.Trim();
            personReferenceOriginal.LastName = lastname.Trim();
            personReferenceOriginal.Gender = gender.Trim();
            personReferenceOriginal.MaritalStatus = maritalstatus.Trim();
            personReferenceOriginal.NickName = firstname.Trim();
            personReferenceOriginal.PhoneNumbers = new Contracts.GenericListResult<Contracts.PhoneReference>();
            personReferenceOriginal.PhoneNumbers.Items = new List<Contracts.PhoneReference>();
            // Phone numbers are optional
            if (!String.IsNullOrEmpty(phonehome))
            {
                personReferenceOriginal.PhoneNumbers.Items.Add(new Contracts.PhoneReference(PersonPhone.FormatPhone(phonehome.Trim()), "", "main/home"));
            }
            if (!String.IsNullOrEmpty(phonecell))
            {
                personReferenceOriginal.PhoneNumbers.Items.Add(new Contracts.PhoneReference(PersonPhone.FormatPhone(phonecell.Trim()), "", "cell"));
            }
            personReferenceOriginal.PostalCode = postalcode.Trim();
            personReferenceOriginal.State = state.Trim();
            personReferenceOriginal.Street1 = street1.Trim();
            personReferenceOriginal.Street2 = street2.Trim();

            // What is the minimum matching percent of a person that we will return from the function
            int matchPercentMinimum = String.IsNullOrEmpty(matchpercentminimum) ? 0 : Convert.ToInt32(matchpercentminimum);

            //  Let's determine the provided gender or stop
            int genderId = -1;
            switch (gender.ToLower())
            {
                case "male":
                    genderId = 0;
                    break;
                case "female":
                    genderId = 1;
                    break;
            }

            if (genderId == -1)
            {
                throw new Exception("Please specify a valid gender.");
            }

            // Let's use the first 2 characters in order to limit results and do more auto-matching
            string firstNameInitial = (firstname.Length > 1 ? firstname.Substring(0, 2) : firstname.Substring(0, 1));

            // Get persons by first initial, last name, gender and birthdate
            CorePersonService cps = new CorePersonService();
            List<core_person> persons = cps.GetList(firstNameInitial, lastname, false);

            PersonCollection pc;
            List<Person> personList = new List<Person>();

            // Get persons with this e-mail address
            pc = new PersonCollection();
            pc.LoadByEmail(email);
            personList.AddRange((from pcbe in pc
                                 select pcbe).ToList());

            // Get persons with this phone number
            if (!String.IsNullOrEmpty(phonehome))
            {
                phonehome = PersonPhone.FormatPhone(phonehome);
                pc = new PersonCollection();
                pc.LoadByPhone(phonehome);
                personList.AddRange((from pcbe in pc
                                     select pcbe).ToList());
            }
            if (!String.IsNullOrEmpty(phonecell))
            {
                phonecell = PersonPhone.FormatPhone(phonecell);
                pc = new PersonCollection();
                pc.LoadByPhone(phonecell);
                personList.AddRange((from pcbe in pc
                                     select pcbe).ToList());
            }

            // Get persons with this address and first initial
            Address address = new Address(street1, street2, city, state, postalcode);
            CoreAddressService cas = new CoreAddressService();
            List<int> addressIdList = cas.GetList(address.StreetLine1, address.StreetLine2, address.PostalCode);
            Person person = new Person();

            // TODO: enhance Arena with LoadByAddressIDs function to improve performance
            foreach (int addressId in addressIdList)
            {
                PersonAddressCollection pac = new PersonAddressCollection();
                pac.LoadByAddressID(addressId);

                foreach (PersonAddress pa in pac)
                {
                    person = new Person(pa.PersonID);
                    if ((person.FirstName.ToLower().StartsWith(firstNameInitial.ToLower()) || person.NickName.ToLower().StartsWith(firstNameInitial.ToLower())))
                    {
                        personList.Add(person);
                    }
                }
            }

            // Remove duplicates
            personList = (from p in personList select p).Distinct().ToList();

            // Load persons over the Match Percent Minimum threshold
            List<int> personIdList = new List<int>();
            Contracts.PersonReference personReference = new Contracts.PersonReference();
            string mapMatch = String.Empty;
            int counter = 0;

            foreach (Person p in personList)
            {
                if ((from prl in personIdList where prl == p.PersonID select prl).Count() == 0)
                {
                    counter++;
                    personReference = ConvertPersonToPersonReference(p);
                    personReference.MatchPercent = CalculateMatchPercent(personReferenceOriginal, personReference, out mapMatch);
                    personReference.MatchMap = mapMatch;
                    personReference.IsExact = "false";
                    // If Person is greater than Match Percent Minimum then check for exact match and add to
                    if (Convert.ToInt32(personReference.MatchPercent) >= matchPercentMinimum)
                    {
                        string domain = String.Empty;
                        personReference.IsExact = IsPersonQualifiedForExact(p, (personReference.MaritalStatus == personReferenceOriginal.MaritalStatus ? true : false), birthdate, email, firstname, lastname, gender, street1, street2, city, state, postalcode, phonehome, phonecell, out domain).ToString();
                        personReference.IsExactPrimaryEmailDomain = domain;
                        personReferenceList.Items.Add(personReference);
                        personIdList.Add(personReference.PersonId);
                    }
                }
            }

            foreach (core_person cp in persons)
            {
                if ((from prl in personIdList where prl == cp.person_id select prl).Count() == 0)
                {
                    counter++;
                    person = new Person(cp.person_id);
                    personReference = ConvertPersonToPersonReference(person);
                    personReference.IsExact = "false";
                    personReference.MatchPercent = CalculateMatchPercent(personReferenceOriginal, personReference, out mapMatch);
                    personReference.MatchMap = mapMatch;
                    // If Person is greater than Match Percent Minimum then check for exact match
                    if (Convert.ToInt32(personReference.MatchPercent) >= matchPercentMinimum)
                    {
                        string domain = String.Empty;
                        personReference.IsExact = IsPersonQualifiedForExact(person, (personReference.MaritalStatus == personReferenceOriginal.MaritalStatus ? true : false), birthdate, email, firstname, lastname, gender, street1, street2, city, state, postalcode, phonehome, phonecell, out domain).ToString();
                        personReference.IsExactPrimaryEmailDomain = domain;                        
                        personReferenceList.Items.Add(personReference);
                        personIdList.Add(personReference.PersonId);
                    }
                }
            }

            // Order filtered persons by Match Percent
            Contracts.GenericListResult<Contracts.PersonReference> personsSorted = new Contracts.GenericListResult<Contracts.PersonReference>();
            personsSorted.Items = new List<Contracts.PersonReference>();
            personsSorted.Items = (from prl in personReferenceList.Items orderby Convert.ToInt32(prl.MatchPercent) descending select prl).ToList();
            
            personsSorted.Max = counter;
            personsSorted.Total = personReferenceList.Items.Count();

            return personsSorted;
        }

        // Determine if Person has exact match and if so return the primary domain of the matched person
        private bool IsPersonQualifiedForExact(Person person, bool maritalstatusmatch, string birthdate, string email, string firstname, string lastname, string gender, string street1, string street2, string city, string state, string postalcode, string phonehome, string phonecell, out string domain)
        {
            domain = String.Empty;
            int mismatchCounter = 0;

            // EMAIL
            // If email is not exact match INCREMENT
            // Do this first in order to get the e-mail domain for e-mail notifications (ie, "An important e-mail was sent to your email account at DOMAIN.EXT")
            bool flag = false;
            foreach (PersonEmail personEmail in person.Emails)
            {
                if (String.IsNullOrEmpty(domain))
                {
                    domain = personEmail.Email.Substring(personEmail.Email.IndexOf("@") + 1, personEmail.Email.Length - (personEmail.Email.IndexOf("@") + 1));
                }
                if (email.ToLower() == personEmail.Email.ToLower())
                {
                    flag = true;
                }
            }
            if (flag == false)
            {
                mismatchCounter++;
            }

            // LAST NAME
            // If person's last name is different DISQUALIFY
            if (lastname.ToLower() != person.LastName.ToLower())
            {
                mismatchCounter = 2;
                return false;
            }

            // FIRST NAME
            // If person's first name is different from first name and nick name DISQUALIFY
            if (firstname.ToLower() != person.FirstName.ToLower() && firstname.ToLower() != person.NickName.ToLower())
            {
                mismatchCounter = 2;
                return false;
            }

            // ADDRESS: Part 1
            flag = false;
            Address a = new Address(street1, street2, city, state, postalcode);

            // If address could not be standardized DISQUALIFY
            if (!a.Standardized)
            {
                mismatchCounter = 2;
                return false;
            }

            // MARITAL STATUS
            // If marital status does not match DISQUALIFY
            if (!maritalstatusmatch)
            {
                mismatchCounter = 2;
                return false;
            }

            // GENDER
            // If gender is different DISQUALIFY
            if (gender.ToLower() != person.Gender.ToString().ToLower())
            {
                mismatchCounter = 2;
                return false;
            }

            // RECORD STATUS
            // If person record is not active DISQUALIFY
            if (person.RecordStatus != Enums.RecordStatus.Active)
            {
                mismatchCounter = 2;
                return false;
            }

            // BIRTH DATE
            // If person's birth date is different INCREMENT
            if (Convert.ToDateTime(birthdate) != Convert.ToDateTime(person.BirthDate.ToString("yyyy/MM/dd")))
            {
                mismatchCounter++;
            }

            // ADDRESS: Part 2
            // If address is not exact match INCREMENT
            foreach (PersonAddress address in person.Addresses)
            {
                // Exclude city and state as zip code's may have several valid cities
                if (a.StreetLine1 == address.Address.StreetLine1 && a.StreetLine2 == address.Address.StreetLine2 && address.Address.PostalCode == a.PostalCode)
                {
                    flag = true;
                }
            }
            if (flag == false)
            {
                mismatchCounter++;
            }

            // PHONE
            // If phone is not exact match INCREMENT
            flag = false;
            foreach (PersonPhone personPhone in person.Phones)
            {
                if (phonehome == personPhone.Number || phonecell == personPhone.Number)
                {
                    flag = true;
                }
            }
            if (flag == false)
            {
                mismatchCounter++;
            }

            // If 2 or more mismatches DISQUALIFY
            if (mismatchCounter < 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Loads an PersonReference object with the corresponding values in the Person object
        /// </summary>
        /// <param name="person"></param>
        /// <returns>PersonReference - Object</returns>
        public Contracts.PersonReference ConvertPersonToPersonReference(Person person)
        {
            Contracts.PersonReference personReference = new Contracts.PersonReference();

            try
            {
                personReference.Age = person.Age;
            }
            catch { personReference.Age = 0; }
            try
            {
                personReference.AnniversaryDate = person.AnniversaryDate == null || person.AnniversaryDate == Convert.ToDateTime("1900-01-01 00:00:00.000") ? String.Empty : person.AnniversaryDate.ToShortDateString();
            }
            catch { personReference.AnniversaryDate = String.Empty; }
            try
            {
                personReference.BirthDate = person.BirthDate == null || person.BirthDate == Convert.ToDateTime("1900-01-01 00:00:00.000") ? String.Empty : person.BirthDate.ToShortDateString();
            }
            catch { personReference.BirthDate = String.Empty; }
            try
            {
                personReference.Campus = person.Campus.Name;
            }
            catch { personReference.Campus = String.Empty; }
            try
            {
                personReference.City = person.PrimaryAddress.City;
            }
            catch { personReference.City = String.Empty; }

            personReference.FamilyId = person.FamilyId;
            try
            {
                personReference.FamilyIsChild = "false";
                FamilyMember fm = new FamilyMember(person.FamilyId, person.PersonID);
                if (fm.FamilyRole.Value.ToLower() == "child")
                {
                    personReference.FamilyIsChild = "true";
                }
            }
            catch { personReference.FamilyIsChild = "false"; }

            personReference.FirstName = person.FirstName;
            personReference.LastName = person.LastName;
            personReference.MaritalStatus = person.MaritalStatus.Value;
            personReference.NickName = person.NickName;
            personReference.PersonId = person.PersonID;
            personReference.PostalCode = person.PostalCode;
            try
            {
                int peCounter = 0;
                personReference.Emails = new Contracts.GenericListResult<Contracts.EmailReference>();
                personReference.Emails.Items = new List<Contracts.EmailReference>();
            
                foreach(PersonEmail pe in person.Emails)
                {
                    peCounter++;
                    personReference.Emails.Items.Add(new Contracts.EmailReference(pe.EmailId, pe.Email, pe.Active.ToString(), pe.Order.ToString()));
                }
                personReference.Emails.Max = personReference.Emails.Items.Count;
                personReference.Emails.Total = personReference.Emails.Items.Count;
            }
            catch { }
            try
            {
                int ppCounter = 0;
                personReference.PhoneNumbers = new Contracts.GenericListResult<Contracts.PhoneReference>();
                personReference.PhoneNumbers.Items = new List<Contracts.PhoneReference>();

                foreach (PersonPhone pp in person.Phones)
                {
                    ppCounter++;
                    personReference.PhoneNumbers.Items.Add(new Contracts.PhoneReference(pp.Number, pp.Extension, pp.PhoneType.Value));
                }
                personReference.PhoneNumbers.Max = personReference.PhoneNumbers.Items.Count;
                personReference.PhoneNumbers.Total = personReference.PhoneNumbers.Items.Count;
            }
            catch { }

            personReference.RecordStatusActive = person.RecordStatus == Enums.RecordStatus.Active ? true : false;

            try
            {
                personReference.SpouseFirstName = person.Spouse().FirstName;
            }
            catch { personReference.SpouseFirstName = String.Empty; }
            try
            {
                personReference.SpouseNickName = person.Spouse().NickName;
            }
            catch { personReference.SpouseNickName = String.Empty; }

            return personReference;
        }        

        /// <summary>
        /// Calculates a percent corrsponding to the number of matching elements between two PersonReference objects.
        /// </summary>
        /// <param name="personOrig"></param>
        /// <param name="person"></param>
        /// <param name="matchMap"></param>
        /// <returns>Match Percentage</returns>
        private string CalculateMatchPercent(Contracts.PersonReference personOrig, Contracts.PersonReference person, out string matchMap)
        {
            bool doNotAddTwiceFlag = true;
            double percent = 0;
            double capacity = 0;
            List<int> percentList = new List<int>();
            string[] matchMapArray = new string[]{"0","0","0","0","0","0","0","0","0","0","0","0","0","0","0"};

            Person personObj = new Person(person.PersonId);

            try
            {
                capacity = capacity + 2;
                foreach (PersonEmail email in personObj.Emails)
                {
                    try
                    {
                        if (personOrig.Emails.Items[0].Address.ToLower() == email.Email.ToLower())
                        {
                            if (doNotAddTwiceFlag)
                            {
                                percentList.Add(2);
                            }
                            doNotAddTwiceFlag = false;
                            
                            matchMapArray[0] = "1";
                        }
                    }
                    catch { }
                }
                doNotAddTwiceFlag = true;

                capacity = capacity + 2;
                if (personOrig.FirstName.ToLower() == person.FirstName.ToLower() || personOrig.FirstName.ToLower() == person.NickName.ToLower())
                {
                    percentList.Add(2);
                    matchMapArray[1] = "1";
                }

                capacity = capacity + 3;
                if (personOrig.LastName.ToLower() == person.LastName.ToLower())
                {
                       percentList.Add(3);
                       matchMapArray[2] = "1";
                }

                capacity = capacity + 3;
                try
                {
                    if (personOrig.Gender.ToString().ToLower() == personObj.Gender.ToString().ToLower())
                    {
                        percentList.Add(3);
                        matchMapArray[3] = "1";
                    }
                }
                catch { }

                capacity = capacity + 1;
                if (personOrig.MaritalStatus.ToLower() == person.MaritalStatus.ToLower())
                {
                    percentList.Add(1);
                    matchMapArray[4] = "1";
                }

                capacity = capacity + 1;
                if (personOrig.Campus.ToLower() == person.Campus.ToLower())
                {
                    percentList.Add(1);
                    matchMapArray[5] = "1";
                }


                // Loop through the original person's phone numbers
                foreach (Contracts.PhoneReference phoneOrig in personOrig.PhoneNumbers.Items)
                {
                    if (phoneOrig.Type.ToLower() == "main/home")
                    {
                        capacity = capacity + 2;                
                    }
                    else if (phoneOrig.Type.ToLower() == "cell")
                    {
                        capacity = capacity + 2;
                    }
                    try
                    {
                        // Loop through the comparison person's phone numbers
                        foreach (PersonPhone phone in personObj.Phones)
                        {
                            // If phone numbers of each person match
                            if (PersonPhone.StripPhone(phoneOrig.Number) == PersonPhone.StripPhone(phone.Number))
                            {
                                if (phoneOrig.Type.ToLower() == "main/home")
                                {
                                    matchMapArray[6] = "1";
                                }
                                if (phoneOrig.Type.ToLower() == "cell")
                                {
                                    matchMapArray[7] = "1";
                                }
                            }
                        }
                    }
                    catch { }
                }
                percentList.Add(matchMapArray[6] == "1" ? 2 : 0);
                percentList.Add(matchMapArray[7] == "1" ? 2 : 0);

                capacity = capacity + 3;
                try
                {
                    DateTime bDayOrig, bDay;
                    bDayOrig = Convert.ToDateTime(personOrig.BirthDate);
                    bDay = Convert.ToDateTime(person.BirthDate);
                    if (bDayOrig == bDay)
                    {
                        percentList.Add(3);

                        matchMapArray[8] = "1";
                    }
                }
                catch { }

                // If street address is exact match
                capacity = capacity + 2;
                Address address = new Address();
                Address addressOrig = new Address(personOrig.Street1, personOrig.Street2, personOrig.City, personOrig.State, person.PostalCode);

                foreach (PersonAddress personAddress in personObj.Addresses)
                {
                    address = new Address();
                    address = personAddress.Address;

                    if (addressOrig.StreetLine1.ToLower() == address.StreetLine1.ToLower()
                        && addressOrig.StreetLine2.ToLower() == address.StreetLine2.ToLower()
                        && address.PostalCode.ToLower().StartsWith(addressOrig.PostalCode.ToLower()))
                    {
                        if (doNotAddTwiceFlag)
                        {
                            percentList.Add(2);
                        }
                        doNotAddTwiceFlag = false;
                    }
                    if (addressOrig.StreetLine1.ToLower() == address.StreetLine1.ToLower())
                    {
                        matchMapArray[9] = "1";
                    }
                    if (addressOrig.StreetLine2.ToLower() == address.StreetLine2.ToLower())
                    {
                        matchMapArray[10] = "1";
                    }
                    if (addressOrig.City.ToLower() == address.City.ToLower())
                    {
                        matchMapArray[11] = "1";
                    }
                    if (addressOrig.State.ToLower() == address.State.ToLower())
                    {
                        matchMapArray[12] = "1";
                    }
                    if (address.PostalCode.ToLower().StartsWith(addressOrig.PostalCode.ToLower()))
                    {
                        matchMapArray[13] = "1";
                    }
                }
                doNotAddTwiceFlag = true;

                if (person.RecordStatusActive != personOrig.RecordStatusActive)
                {
                    matchMapArray[14] = "1";
                }
            }
            catch { }

            // Set Match Map and add leading zeroes if missing
            matchMap = String.Join(String.Empty, matchMapArray);
            foreach (int i in percentList)
            {
                percent += i;
            }
            return (percent > 0 ? Math.Round((percent / capacity) * 100) : 0).ToString();
        }


        private static string CapitalizeIfNeeded(string subject)
        {
            // Turned this off since we get actual values from middleware, but still here if we need it.
            return subject; //(subject == subject.ToUpper() || subject == subject.ToLower() ? subject.Capitalize() : subject ;
        }
        #endregion
    }
}