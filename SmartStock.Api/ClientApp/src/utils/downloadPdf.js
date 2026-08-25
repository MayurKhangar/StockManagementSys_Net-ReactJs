import invoiceApi from '../api/invoiceApi';

export async function downloadInvoicePdf(invoiceId, invoiceNumber) {
  const { data } = await invoiceApi.downloadPdf(invoiceId);
  const url = window.URL.createObjectURL(new Blob([data], { type: 'application/pdf' }));
  const link = document.createElement('a');
  link.href = url;
  link.setAttribute('download', `${invoiceNumber}.pdf`);
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
}
