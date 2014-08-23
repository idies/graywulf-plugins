graywulf-plugins
================

Extensions to Graywulf, mainly DIBBS integration
=======
rest-client
===========

A very simplistic REST client for .Net
=======
keystone-client
===============

A .Net client to the OpenStack Keystone REST interface.

**Installing the test environment:**

The Keystone client requires a fully funtional Keystone server for testing. The best practice is to install it on a virtual server running some flavour of linux. Below is a walkthrough of installing Keystone on ubuntu.

1. Create a minimal virtual virtual machine and install ubuntu (tested with ubuntu desktop TLS 14.í4)

2. Install the required packages

   > sudo apt-get install mysql-server python-mysqldb keystone

3. Configure MySQL to listen on all network interfaces. This way the databases will be accessible from the developer machine and no need to set up X on the virtual machine.

   - edit etc/mysql/my.cnf
   - comment out line "bind-address..."
   - allow access for the root from remote hosts (unsafe, but this is just a developer environment on a virtual machine); from the mysql prompt enter
   
   mysql> GRANT ALL PRIVILEGES on *.* to root@'%' IDENTIFIED BY 'password';
   
   - verify that you can access the MySQL instance from MySQL Workbench running on the developer machine
   
4. Create a new database for Keystone data and grant necessary permission

   mysql> CREATE DATABASE keystone;
   mysql> GRANT ALL ON keystone.* TO 'keystone'@'%' IDENTIFIED BY 'password';
   mysql> GRANT ALL ON keystone.* TO 'keystone'@'localhost' IDENTIFIED BY 'password';
   
   - note, that it is necessary to create users both for localhost and for %.
   - verify that you can see the database from MySQL Workbench
  
5. Modify keystone configuration to use MySQL

   - edit less /etc/keystone/keystone.conf
   - change connection line to the following
  
   connection = mysql://keystone:password@localhost/keystone
  
6. Create keystone database schema

   > keystone-manage db_sync
  
7. Set up admin token

  - modify the keystone config to have a matching admin token with the one used by test routines
  - edit less /etc/keystone/keystone.conf
  - change admin token to the following
  
  admin_token=e5b19f25f5d55a995a16
  
8. Change token provider to UUID from PKI

   PKI (public-key infrastucture) is a mechanism to produce potentially more secure, verifiable but large tokens. UUID tokens are less secure but much easier to deal with.
   
   Change appropriate line under [token] /etc/keystone/keystone.conf to
   
9. Create admin role and admin user

   Execute the following commands
   
   > keystone role-create --name admin
   > keystone tenant-create --name admin
   > keystone user-create --name admin
   > keystone user-role-add --user admin --role admin
   
   The file /etc/keystone/policy.json should by default contain the following lines:
   
   "admin_required": "role:admin or is_admin:1",
   ...
   "identity:get_user": "rule:admin_required",
   etc.
  
9. Build the solution and run all unit tests.

For more information, see:
* http://www.ubuntu.com/sites/www.ubuntu.com/files/active/Ubuntu_Keystone_WP_WEB_AW.pdf

**About trusts**

Trusts can only be made by trustors, consequently the Keystone API expects a token identifying the trustor and not that of the admin. Always set the UserAuthToken property before calling functions related to trusts.
