#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/Voucher/org/#{orgId}/action/AddVoucher"
param =  {
  VoucherNo: "VC-002",
  Description: "Test voucher",
  Tags: "email=xxxx@gmail.com",
  RedeemPrice: "100.00",
}

result = make_request(:post, apiUrl, param)
puts(result)
