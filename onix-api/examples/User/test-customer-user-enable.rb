#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = '6abfdfa6-0972-46aa-a78d-a6c50051c500'
api = 'EnableCustomerUserById'

param =  nil

### UpdateUserById
apiUrl = "api/Customer/org/#{orgId}/action/#{api}/#{id}"
result = make_request(:post, apiUrl, param)

puts(result)
