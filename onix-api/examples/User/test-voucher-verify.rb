#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

ENV['API_KEY'] = nil # no authen

orgId = ENV['API_ORG']
barcode = 'OV-35937-222622'
voucherNo = 'OV-35937'
pin = '222622'

apiUrl = "api/Voucher/org/#{orgId}/action/VerifyVoucherByBarcode/#{barcode}"
param = nil

result = make_request(:post, apiUrl, param)
puts(result)

apiUrl = "api/Voucher/org/#{orgId}/action/VerifyVoucherByPin/#{voucherNo}/#{pin}"
result = make_request(:post, apiUrl, param)
puts(result)

