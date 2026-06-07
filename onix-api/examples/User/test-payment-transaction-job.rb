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

### Get Payment Transaction Job By ID
paymentTransactionId = "YOUR_PAYMENT_TRANSACTION_ID"  # Replace with actual ID
jobId = "YOUR_JOB_ID"  # Replace with actual ID
apiUrl = "api/PaymentTransaction/org/#{orgId}/action/GetPaymentTransactionJobById/#{paymentTransactionId}/#{jobId}"

token = File.read(keyFile)

ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = token

puts("===== Get Payment Transaction Job By ID =====")
result = make_request(:get, apiUrl)
puts(result)
