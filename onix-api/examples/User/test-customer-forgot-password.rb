#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = "b608a7d1-f954-44b1-baa2-fca8b7a12a57"

apiUrl = "api/Customer/org/#{orgId}/action/SendCustomerResetPasswordEmail/#{id}"

result = make_request(:post, apiUrl, nil)
puts(result)
