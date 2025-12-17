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
voucherId = '7addeff2-aed8-42b5-a311-e7ebff1d2b9c'
pin = '849065'

apiUrl = "api/Voucher/org/#{orgId}/action/ApproveVoucherUsedById/#{voucherId}/#{pin}"
param = nil

result = make_request(:post, apiUrl, param)
puts(result)

