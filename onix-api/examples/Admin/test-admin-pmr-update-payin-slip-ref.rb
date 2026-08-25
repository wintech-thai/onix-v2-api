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
slipId = "ใส่-uuid-ของ-slip-ที่ต้องการแก้ไขที่นี่"

###
apiUrl = "admin-api/AdminPaymentRequest/org/#{orgId}/action/UpdatePayInSlipFirst4Last4/#{paymentRequestId}/#{slipId}"
param = {
  First4: "AA11",
  Last4: "ZZ99",
  Note: "แก้ไขโดย admin เพื่อให้ตรงกับสลิปจริง",
}

token = File.read(keyFile)

ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = token

result = make_request(:patch, apiUrl, param)
puts(result.to_json)
