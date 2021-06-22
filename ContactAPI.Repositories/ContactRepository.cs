using DataAccess;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactAPI.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly AppDbContext _appDbContext;

        public ContactRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;

        }
        public Contact CreateContact(string firstName, string lastName, string email, string address, string phoneNumber, string photoPath)
        {
            return new Contact
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Address = address,
                PhoneNumber = phoneNumber,
                PhotoPath = photoPath,
            };
        }
        public async Task<bool> AddContact(Contact contact)
        {
            bool isSuccess = false;
            int check = 0;


            await _appDbContext.AddAsync(contact);

            check = await _appDbContext.SaveChangesAsync();

            if (check >= 1) isSuccess = true;


            return isSuccess;
        }

        public async Task<bool> AddListOfContacts(List<Contact> listOfContacts)
        {
            bool isSuccess = false;
            int check = 0;

            await _appDbContext.AddRangeAsync(listOfContacts);

            check = await _appDbContext.SaveChangesAsync();

            if (check >= 1) isSuccess = true;


            return isSuccess;
        }

        public async Task<bool> DeleteContact(Contact contact)
        {
            bool isSuccess = false;
            int check = 0;


            _appDbContext.Contacts.Remove(contact);

            check = await _appDbContext.SaveChangesAsync();

            if (check >= 1) isSuccess = true;

            return isSuccess;
        }

        public Contact GetContactByEmail(string email)
        {

            Contact contact = _appDbContext.Contacts.FirstOrDefault(ct => ct.Email == email);
            if (contact == null) throw new Exception("Contact does not exist");

            return contact;
        }

        public Contact GetContactById(string id)
        {
            Contact contact = _appDbContext.Contacts.FirstOrDefault(ct => ct.Id == id);
            if (contact == null) throw new Exception("Contact does not exist");

            return contact;
        }

        public List<Contact> GetAllContacts()
        {

            return _appDbContext.Contacts.Select(cont => cont).ToList();
        }


        public Contact GetContactByQuery(string query)
        {
            var contactToReturn = _appDbContext.Contacts.FirstOrDefault(x => x.Email == query || x.Id == query || x.PhoneNumber == query);
            return contactToReturn;
        }

        public async Task<bool> UpdateContact(Contact contact)
        {
            bool isSuccess = false;
            int check = 0;

            var contactToUpdate = _appDbContext.Contacts.FirstOrDefault(ct => ct.Id == contact.Id);

            _appDbContext.Entry(contactToUpdate).CurrentValues.SetValues(contact);

            check = await _appDbContext.SaveChangesAsync();

            if (check >= 1) isSuccess = true;

            return isSuccess;
        }

        public bool ContactExists(string phoneNumber, string firstName)
        {
            return _appDbContext.Contacts.Any(contact => contact.FirstName == firstName && contact.PhoneNumber == phoneNumber);
        }
    }
}
