using Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ContactAPI.Repositories
{
    public interface IContactRepository
    {
        Task<bool> AddContact(Contact contact);
        Task<bool> AddListOfContacts(List<Contact> listOfContacts);
        List<Contact> GetAllContacts();
        Contact GetContactById(string id);
        Contact GetContactByEmail(string email);
        Task<bool> UpdateContact(Contact contact);
        Task<bool> DeleteContact(Contact contact);
        bool ContactExists(string phoneNumber, string firstName);
        Contact CreateContact(string firstName, string lastName, string email, string address, string phoneNumber, string photoPath);
        Contact GetContactByQuery(string query);
    }
}
