#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = "342c95db-6f48-459f-9f74-f0eff6eea71c"

apiUrl = "api/Customer/org/#{orgId}/action/SendCustomerResetPasswordEmail/#{id}"

result = make_request(:post, apiUrl, nil)
puts(result)
