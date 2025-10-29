#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']

param =  {
  FullTextSearch: ""
}

apiUrl = "api/Customer/org/#{orgId}/action/GetCustomers"
result = make_request(:post, apiUrl, param)
puts(result)

apiUrl = "api/Customer/org/#{orgId}/action/GetCustomerCount"
result = make_request(:post, apiUrl, param)
puts(result)
