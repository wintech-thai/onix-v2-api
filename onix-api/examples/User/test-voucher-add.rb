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
  CustomerId: "342c95db-6f48-459f-9f74-f0eff6eea71c",
  PrivilegeId: "a4688dd4-936d-4931-aef9-898dcd90f9e9",
  VoucherNo: "VC-005",
  Description: "Test voucher",
  Tags: "email=xxxx@gmail.com",
  RedeemPrice: "100.00",
}

result = make_request(:post, apiUrl, param)
puts(result)
