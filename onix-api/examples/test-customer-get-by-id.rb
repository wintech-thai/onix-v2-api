#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']
id = '349788f4-64ba-4d65-9a97-0417a70294d8'

param =  nil

### GetCustomerById
apiUrl = "api/Customer/org/#{orgId}/action/GetCustomerById/#{id}"
result = make_request(:get, apiUrl, param)

puts(result)
