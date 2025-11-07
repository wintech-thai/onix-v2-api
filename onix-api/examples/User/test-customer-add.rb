#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

param =  {
  Code: "CUST-004",
  Name: "พี่เจมส์ รักษ์ธรรมชาติ นะจ๊ะ",
  PrimaryEmail: "abcde4@xyz.com",
}

### Inviteuser
apiUrl = "api/Customer/org/#{orgId}/action/AddCustomer"
result = make_request(:post, apiUrl, param)
puts(result)
