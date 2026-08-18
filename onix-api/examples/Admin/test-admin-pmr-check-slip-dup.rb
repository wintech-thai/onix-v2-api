#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

keyFile = ".token"
orgId = "ppm-alfa999"
paymentRequestId = "3fa85f64-5717-4562-b3fc-2c963f66afa6"
first4 = "AA11"
last4  = "ZZ99"

###
apiUrl = "admin-api/AdminPaymentRequest/org/#{orgId}/action/CheckPayInSlipDup/#{paymentRequestId}/#{first4}/#{last4}"
param = nil

token = File.read(keyFile)

ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = token

result = make_request(:get, apiUrl, param)
puts(result.to_json)
