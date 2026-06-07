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

### Get Payment Documents
apiUrl = "api/PaymentDocument/org/#{orgId}/action/GetPaymentDocuments"
param =  {
  FullTextSearch: "",
  Status: "",
  Limit: 10,
  Offset: 0
}

token = File.read(keyFile)

ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = token

puts("===== Get Payment Documents =====")
result = make_request(:post, apiUrl, param)
puts(result)

### Get Payment Document Count
apiUrl = "api/PaymentDocument/org/#{orgId}/action/GetPaymentDocumentCount"

puts("\n===== Get Payment Document Count =====")
result = make_request(:post, apiUrl, param)
puts(result)
