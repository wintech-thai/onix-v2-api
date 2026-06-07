#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
keyFile = ".token"

### Get Payment Requests
apiUrl = "api/PaymentRequest/org/#{orgId}/action/GetPaymentRequests"
param =  {
  FullTextSearch: "",
  Status: "",
  Limit: 10,
  Offset: 0
}

token = File.read(keyFile)

ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = token

puts("===== Get Payment Requests =====")
result = make_request(:post, apiUrl, param)
puts(result)

### Get Payment Request Count
apiUrl = "api/PaymentRequest/org/#{orgId}/action/GetPaymentRequestCount"

puts("\n===== Get Payment Request Count =====")
result = make_request(:post, apiUrl, param)
puts(result)
