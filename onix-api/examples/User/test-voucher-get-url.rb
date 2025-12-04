#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = 'aadd8ff1-03e2-427e-a6e6-8d0ec4940389'

apiUrl = "api/Voucher/org/#{orgId}/action/GetVoucherVerifyQrUrl/#{id}"
param = nil

result = make_request(:get, apiUrl, param)
puts(result)

apiUrl = "api/Voucher/org/#{orgId}/action/GetVoucherVerifyUrl"
result = make_request(:get, apiUrl, param)
puts(result)
