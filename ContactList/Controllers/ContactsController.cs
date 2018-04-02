﻿using ContactList.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Net.Http;

namespace ContactList.Controllers
{
    public class ContactsController : ApiController
    {
        private const string FILENAME = "contacts.json";
        private GenericStorage _storage;

        public ContactsController()
        {
            _storage = new GenericStorage();
        }

        private async Task<IEnumerable<Contact>> GetContacts()
        {
            var contacts = await _storage.Get(FILENAME);

            if (contacts == null)
            {
                await _storage.Save(new Contact[]{
                        new Contact { Id = 1, EmailAddress = "barney@contoso.com", Name = "Barney Poland"},
                        new Contact { Id = 2, EmailAddress = "lacy@contoso.com", Name = "Lacy Barrera"},
                        new Contact { Id = 3, EmailAddress = "lora@microsoft.com", Name = "Lora Riggs"}
                    }
                , FILENAME);
            }

            return contacts;
        }

        /// <summary>
        /// Gets the list of contacts
        /// </summary>
        /// <returns>The contacts</returns>
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK,
            Type = typeof(IEnumerable<Contact>))]
        [Route("~/contacts")]
        public async Task<IEnumerable<Contact>> Get()
        {
            return await GetContacts();
        }

        /// <summary>
        /// Gets a specific contact
        /// </summary>
        /// <param name="id">Identifier for the contact</param>
        /// <returns>The requested contact</returns>
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK,
            Description = "OK",
            Type = typeof(IEnumerable<Contact>))]
        [SwaggerResponse(HttpStatusCode.NotFound,
            Description = "Contact not found",
            Type = typeof(IEnumerable<Contact>))]
        [SwaggerOperation("GetContactById")]
        [Route("~/contacts/{id}")]
        public async Task<Contact> Get([FromUri] int id)
        {
            var contacts = await GetContacts();
            return contacts.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Creates a new contact
        /// </summary>
        /// <param name="contact">The new contact</param>
        /// <returns>The saved contact</returns>
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.Created,
            Description = "Created",
            Type = typeof(Contact))]
        [Route("~/contacts")]
        public async Task<Contact> Post([FromBody] Contact contact)
        {
            var contacts = await GetContacts();
            var contactList = contacts.ToList();
            contactList.Add(contact);
            await _storage.Save(contactList, FILENAME);
            return contact;
        }

        /// <summary>
        /// Deletes a contact
        /// </summary>
        /// <param name="id">Identifier of the contact to be deleted</param>
        /// <returns>True if the contact was deleted</returns>
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.OK,
            Description = "OK",
            Type = typeof(bool))]
        [SwaggerResponse(HttpStatusCode.NotFound,
            Description = "Contact not found",
            Type = typeof(bool))]
        [Route("~/contacts/{id}")]
        public async Task<HttpResponseMessage> Delete([FromUri] int id)
        {
            var contacts = await GetContacts();
            var contactList = contacts.ToList();

            if (!contactList.Any(x => x.Id == id))
            {
                return Request.CreateResponse<bool>(HttpStatusCode.NotFound, false);
            }
            else
            {
                contactList.RemoveAll(x => x.Id == id);
                await _storage.Save(contactList, FILENAME);
                return Request.CreateResponse<bool>(HttpStatusCode.OK, true);
            }
        }
    }
}