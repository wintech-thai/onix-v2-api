#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
voucherId = 'aadd8ff1-03e2-427e-a6e6-8d0ec4940389'
pin = '222622'

apiUrl = "api/Voucher/org/#{orgId}/action/SetVoucherUnusedById/#{voucherId}"
param = nil

result = make_request(:post, apiUrl, param)
puts(result)

