#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = '375ed468-0eb1-4bb1-81ae-4b33276a3889'
email = 'abcde2@xyz.com'
api = 'UpdateCustomerEmailWithConfirmationById' # UpdateCustomerEmailById

param =  nil

### UpdateUserById
apiUrl = "api/Customer/org/#{orgId}/action/#{api}/#{id}/#{email}"
result = make_request(:post, apiUrl, param)

puts(result)
