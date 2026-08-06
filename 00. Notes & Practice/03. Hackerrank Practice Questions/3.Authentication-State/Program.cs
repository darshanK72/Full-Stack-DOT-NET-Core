using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using System;

interface IUser
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int IncorrectAttempt { get; set; }
    public string Location { get; set; }
}

interface IApplicationAuthState
{
    public List<IUser> RegisteredUsers { get; set; }
    public List<IUser> UsersLoggedIn { get; set; }
    public List<string> AllowedLocations { get; set; }
    string Register(IUser user);
    string Login(IUser user);
    string Logout(IUser user);
}

/*
 * These strings can be copied and pasted to avoid typing errors.
 * User1@email.com should be replaced with the correct user email.
 *
 * User1@email.com registered successfully! **
 * User1@email.com is not registered! **
 * User1@email.com is not logged in! **
 * User1@email.com is already registered! **
 * User1@email.com logged in successfully! **
 * User1@email.com logged out successfully! **
 * User1@email.com is already logged in! **
 * User1@email.com is already logged in from another location! **
 * User1@email.com is blocked!**
 * User1@email.com is not allowed to login from this location! **
 * User1@email.com password is incorrect! **
 */

class User : IUser
{

    public int Id { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int IncorrectAttempt { get; set; }
    public string Location { get; set; }

    public User(int Id, string Email, string Password, string Location)
    {
        this.Id = Id;
        this.Email = Email;
        this.Password = Password;
        this.Location = Location;
        this.IncorrectAttempt = 0;
    }
}

class ApplicationAuthState : IApplicationAuthState
{

    public List<string> AllowedLocations { get; set; }
    public List<IUser> RegisteredUsers { get; set; }
    public List<IUser> UsersLoggedIn { get; set; }

    public ApplicationAuthState(List<string> AllowedLocations)
    {
        this.AllowedLocations = AllowedLocations;
        this.RegisteredUsers = new List<IUser>();
        this.UsersLoggedIn = new List<IUser>();
    }

    public string Register(IUser user)
    {
        if (RegisteredUsers.Any(u => u.Email == user.Email))
        {
            return $"{user.Email} is already registered!";
        }

        RegisteredUsers.Add(user);
        return $"{user.Email} registered successfully!";
    }

    public string Login(IUser user)
    {

        string outmessage = string.Empty;
        IUser regUser = RegisteredUsers.FirstOrDefault(u => u.Email == user.Email);

        if (regUser == null)
        {
            outmessage = $"{user.Email} is not registered!";
        }
        else if (regUser.IncorrectAttempt >= 3)
        {
            regUser.IncorrectAttempt++;
            outmessage = $"{user.Email} is blocked!";
        }
        else if (regUser.Password != user.Password)
        {
            regUser.IncorrectAttempt++;
            outmessage = $"{user.Email} password is incorrect!";
        }
        else if (UsersLoggedIn.Any(u => u.Email == user.Email && u.Location != user.Location))
        {
            // regUser.IncorrectAttempt++;
            outmessage = $"{user.Email} is already logged in from another location!";
        }
        else if (UsersLoggedIn.Any(u => u.Email == user.Email && u.Location == user.Location))
        {
            // regUser.IncorrectAttempt++;
            outmessage = $"{user.Email} is already logged in!";
        }
        else if (!AllowedLocations.Contains(user.Location))
        {
            outmessage = $"{user.Email} is not allowed to login from this location!";
        }
        else if (regUser.Password == user.Password)
        {
            user.IncorrectAttempt = 0;
            this.UsersLoggedIn.Add(user);
            outmessage = $"{user.Email} logged in successfully!";
        }
        return outmessage;

    }

    public string Logout(IUser user)
    {
        if (UsersLoggedIn.Any(u => u.Email == user.Email))
        {
            UsersLoggedIn.Remove(user);
            return $"{user.Email} logged out successfully!";
        }
        return $"{user.Email} is not logged in!";
    }
}

class Solution
{
    public static void Main(string[] args)
    {
        TextWriter textWriter = new StreamWriter(@System.Environment.GetEnvironmentVariable("OUTPUT_PATH"), true);
        List<IUser> users = new List<IUser>();
        List<string> allowedLocations = new List<string>();
        int allowedLocationCount = Convert.ToInt32(Console.ReadLine().Trim());
        for (int i = 0; i < allowedLocationCount; i++)
        {
            var a = Console.ReadLine().Trim();
            allowedLocations.Add(a);
        }
        ApplicationAuthState authState = new ApplicationAuthState(allowedLocations);

        int usersCount = Convert.ToInt32(Console.ReadLine().Trim());
        for (int i = 0; i < usersCount; i++)
        {
            var a = Console.ReadLine().Trim().Split(',');
            users.Add(new User(Convert.ToInt32(a[0]), a[1], a[2], a[3]));
        }

        int actionCount = Convert.ToInt32(Console.ReadLine().Trim());
        for (int i = 0; i < actionCount; i++)
        {
            var a = Console.ReadLine().Trim().Split(':');
            var uIndex = Convert.ToInt32(a[1]);
            if (a[0] == "Register")
            {
                textWriter.WriteLine(authState.Register(users[uIndex]));
            }
            else if (a[0] == "Login")
            {
                textWriter.WriteLine(authState.Login(users[uIndex]));
            }
            else if (a[0] == "Logout")
            {
                textWriter.WriteLine(authState.Logout(users[uIndex]));
            }
        }

        textWriter.Flush();
        textWriter.Close();
    }
}
