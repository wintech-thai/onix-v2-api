#!/usr/bin/env ruby
# ทดสอบ CheckPayInSlipDup (AllowAnonymous)
# paymentRequestId ที่ส่งไปจะถูก exclude ออกจากผลลัพธ์ (ไม่นับตัวเอง)

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId            = "ppm-alfa999"
paymentRequestId = "3fa85f64-5717-4562-b3fc-2c963f66afa6"  # exclude request นี้จาก dup
first4 = "AA11"
last4  = "ZZ99"

###
apiUrl = "api/PaymentRequest/org/#{orgId}/action/CheckPayInSlipDup/#{paymentRequestId}/#{first4}/#{last4}"
param = nil

ENV['API_KEY'] = nil      # AllowAnonymous
ENV['ACCESS_TOKEN'] = nil

result = make_request(:get, apiUrl, param)
puts(result.to_json)
