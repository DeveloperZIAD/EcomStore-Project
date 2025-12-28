// File: AddressService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Data_layer;

namespace Business_layer
{
    public static class AddressService
    {
        public static int AddAddress(int userId, clsaddressesdb addressDto)
        {
            ValidateUserId(userId);
            ValidateAddress(addressDto);

            addressDto.user_id = userId;

            if (addressDto.is_default)
            {
                UnsetAllDefaultsForUser(userId);
            }

            int newId = addressesdb_dal.AddAddress(addressDto);

            if (newId <= 0)
                throw new Exception("Failed to add the address.");

            return newId;
        }

        public static List<clsaddressesdb> GetAddressesByUserId(int userId)
        {
            ValidateUserId(userId);
            return addressesdb_dal.GetAddressesByUserId(userId);
        }

        public static clsaddressesdb GetAddressById(int addressId, int userId)
        {
            ValidateUserId(userId);
            if (addressId <= 0) throw new ArgumentException("Invalid address ID.");

            var address = addressesdb_dal.GetAddressById(addressId);

            if (address == null)
                throw new KeyNotFoundException("Address not found.");

            if (address.user_id != userId)
                throw new UnauthorizedAccessException("You are not authorized to access this address.");

            return address;
        }

        public static bool UpdateAddress(int addressId, int userId, clsaddressesdb addressDto)
        {
            ValidateUserId(userId);
            if (addressId <= 0) throw new ArgumentException("Invalid address ID.");
            ValidateAddress(addressDto);

            var existing = addressesdb_dal.GetAddressById(addressId);

            if (existing == null)
                throw new KeyNotFoundException("Address not found.");

            if (existing.user_id != userId)
                throw new UnauthorizedAccessException("You are not authorized to update this address.");

            existing.street = addressDto.street.Trim();
            existing.city = addressDto.city.Trim();
            existing.state = string.IsNullOrWhiteSpace(addressDto.state) ? null : addressDto.state.Trim();
            existing.country = addressDto.country.Trim();
            existing.zip_code = addressDto.zip_code.Trim();

            if (addressDto.is_default && !existing.is_default)
            {
                UnsetAllDefaultsForUser(userId);
            }

            existing.is_default = addressDto.is_default;

            return addressesdb_dal.UpdateAddress(existing);
        }

        public static bool DeleteAddress(int addressId, int userId)
        {
            ValidateUserId(userId);
            if (addressId <= 0) throw new ArgumentException("Invalid address ID.");

            var address = addressesdb_dal.GetAddressById(addressId);

            if (address == null)
                throw new KeyNotFoundException("Address not found.");

            if (address.user_id != userId)
                throw new UnauthorizedAccessException("You are not authorized to delete this address.");

            int userAddressesCount = addressesdb_dal.GetAddressesByUserId(userId).Count;

            if (userAddressesCount <= 1)
                throw new InvalidOperationException("Cannot delete the user's only address.");

            return addressesdb_dal.DeleteAddress(addressId);
        }

        public static bool SetDefaultAddress(int addressId, int userId)
        {
            ValidateUserId(userId);
            if (addressId <= 0) throw new ArgumentException("Invalid address ID.");

            var address = addressesdb_dal.GetAddressById(addressId);

            if (address == null)
                throw new KeyNotFoundException("Address not found.");

            if (address.user_id != userId)
                throw new UnauthorizedAccessException("You are not authorized to set this address as default.");

            UnsetAllDefaultsForUser(userId);

            address.is_default = true;
            return addressesdb_dal.UpdateAddress(address);
        }

        // ----------------- Private Helper Methods -----------------

        private static void ValidateUserId(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.");
        }

        private static void ValidateAddress(clsaddressesdb address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (string.IsNullOrWhiteSpace(address.street)) throw new ArgumentException("Street is required.");
            if (string.IsNullOrWhiteSpace(address.city)) throw new ArgumentException("City is required.");
            if (string.IsNullOrWhiteSpace(address.country)) throw new ArgumentException("Country is required.");
            if (string.IsNullOrWhiteSpace(address.zip_code)) throw new ArgumentException("Zip code is required.");
        }

        private static void UnsetAllDefaultsForUser(int userId)
        {
            var addresses = addressesdb_dal.GetAddressesByUserId(userId);
            foreach (var addr in addresses.Where(a => a.is_default))
            {
                addr.is_default = false;
                addressesdb_dal.UpdateAddress(addr);
            }
        }
    }
}