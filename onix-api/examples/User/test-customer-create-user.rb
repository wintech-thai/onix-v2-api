#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = '342c95db-6f48-459f-9f74-f0eff6eea71c'
api = 'SendCustomerUserCreationEmail'

param =  nil

### UpdateUserById
apiUrl = "api/Customer/org/#{orgId}/action/#{api}/#{id}"
result = make_request(:post, apiUrl, param)

puts(result)
