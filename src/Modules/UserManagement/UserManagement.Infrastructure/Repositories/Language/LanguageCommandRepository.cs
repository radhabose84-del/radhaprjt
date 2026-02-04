using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Infrastructure.Data;
using UserManagement.Application.Common.Interfaces.ILanguage;
using UserManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Infrastructure.Repositories.Language
{
    public class LanguageCommandRepository : ILanguageCommand
    {
        private readonly ApplicationDbContext _context;
        public LanguageCommandRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<UserManagement.Domain.Entities.Language> CreateAsync(UserManagement.Domain.Entities.Language language)
        {
             await _context.Languages.AddAsync(language);
            await _context.SaveChangesAsync();
            return language;
        }

        public async Task<bool> DeleteAsync(int id, UserManagement.Domain.Entities.Language language)
        {
             var existingLanguage = await _context.Languages.FirstOrDefaultAsync(u => u.Id == id && u.IsDeleted==0);
            if (existingLanguage != null)
            {
                existingLanguage.IsDeleted = language.IsDeleted;
                return await _context.SaveChangesAsync() >0;
            }
            return false; 
        }

        public async Task<bool> UpdateAsync(UserManagement.Domain.Entities.Language language)
        {
             var Languages = await _context.Languages.FirstOrDefaultAsync(u => u.Id == language.Id && u.IsDeleted==0);
            if (Languages != null)
            {
                Languages.Code = language.Code;
                Languages.Name = language.Name;
                Languages.IsActive = language.IsActive;

                _context.Languages.Update(Languages);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}