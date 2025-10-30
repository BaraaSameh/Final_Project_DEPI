﻿using DepiFinalProject.Data;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Migrations;
using DepiFinalProject.Models;
using DepiFinalProject.Services;
using Microsoft.EntityFrameworkCore;

namespace DepiFinalProject.Repositories
{
    public class UserRepository: IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> CreateAsync(User user)
        {
            
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.UserEmail == email);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.UserEmail == email);
        }

        public async Task<User> GetByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.UserID== userId);
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
